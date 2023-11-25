using backend.Data;
using backend.RequestModel;
using Docker.DotNet;
using Docker.DotNet.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Diagnostics;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;

namespace backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CompileCodeController : ControllerBase
    {
        [Authorize]
        [HttpPost]
        public IActionResult Create(SourceCode example)
        {
            var cppCode = example.Code;
            string fileName = Guid.NewGuid().ToString(); 
            string filePath = Path.Combine(Directory.GetCurrentDirectory() + "\\Sandbox", fileName + ".cpp");
            string compiledFilePath = Path.Combine(Directory.GetCurrentDirectory() + "\\Sandbox", fileName);
            Console.WriteLine("File path:  " + compiledFilePath);
            try
            {
                System.IO.File.WriteAllText(filePath, cppCode);

                string compilerPath = "g++"; 
                string arguments = $"{filePath} -o {compiledFilePath}";

                ProcessStartInfo compileProcessStartInfo = new ProcessStartInfo
                {
                    FileName = compilerPath,
                    Arguments = arguments,
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (Process compileProcess = new Process())
                {
                    compileProcess.StartInfo = compileProcessStartInfo;
                    compileProcess.Start();
                    compileProcess.WaitForExit();
                }

                string executionResult = string.Empty;

                ProcessStartInfo executionProcessStartInfo = new ProcessStartInfo
                {
                    FileName = compiledFilePath,
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (Process executionProcess = new Process())
                {
                    executionProcess.StartInfo = executionProcessStartInfo;
                    executionProcess.Start();
                    // executionProcess.MaxWorkingSet = new IntPtr(3 * 1024 * 1024);
                    Console.WriteLine(executionProcess.PrivateMemorySize64);
                    
                    bool isProcessStop = executionProcess.WaitForExit(10000);
                    // clockstart

                    if (!isProcessStop)
                    {
                        Console.WriteLine("Not finish yet!");
                        throw new TimeoutException("Execution timed out");
                    }

                    executionResult = executionProcess.StandardOutput.ReadToEnd();
                }

                //using (Process executionProcess = new Process())
                //{
                //    executionProcess.StartInfo = executionProcessStartInfo;
                //    // Use task to run the execution process
                //    long memUsed = 0;
                //    int i = 0;
                //    var task = Task.Run(() =>
                //    {
                //        executionProcess.Start();
                //        executionResult = executionProcess.StandardOutput.ReadToEnd();
                //    });

                //    var completed = task.Wait(TimeSpan.FromSeconds(6));

                //    if (!completed)
                //    {
                //        throw new TimeoutException("Execution timed out.");
                //    }
                //}

                return Ok(executionResult);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error compile code : " + ex);
                return BadRequest("Compile fail!");
            }
            finally
            {
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }

                if (System.IO.File.Exists(compiledFilePath + ".exe"))
                {
                    foreach (var process in Process.GetProcessesByName(fileName))
                    {
                        process.Kill();
                        process.WaitForExit();
                    }

                    System.IO.File.Delete(compiledFilePath + ".exe");

                }
            }

        }


        [HttpPost("docker")]
        public async Task<IActionResult> ExecuteCodeAsync(SourceCode example)
        {
            string DockerfileTemplate = @"
FROM gcc

WORKDIR /app

COPY {0} /app/code.cpp

RUN g++ -o app /app/code.cpp

CMD [""./app""]";

            var _dockerClient = new DockerClientConfiguration().CreateClient();

            //var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            // Currently using as image-tag
            var fileId = Guid.NewGuid().ToString();
            try
            {
                var sourceCode = example.Code;

                // create new folder 
                string folderPath = Path.Combine(Directory.GetCurrentDirectory() + "\\Sandbox", fileId);
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                } else
                {
                    return BadRequest("Error");
                }

                string cppFilePath = Path.Combine(folderPath, fileId + ".cpp");

                await System.IO.File.WriteAllTextAsync(cppFilePath, sourceCode);

                // Add file cpp to docker image for building
                var dockerfileContent = string.Format(DockerfileTemplate, $"{fileId}.cpp");

                // Create file path for docker file
                var dockerfilePath = Path.Combine(folderPath, "Dockerfile");
                await System.IO.File.WriteAllTextAsync(dockerfilePath, dockerfileContent);

                var testDomain = Path.Combine(folderPath);

                var processStartInfo = new ProcessStartInfo
                {
                    FileName = "docker",
                    Arguments = $"build -t {fileId}:latest -f {dockerfilePath} .",
                    WorkingDirectory = testDomain,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                var outputBuilder = new StringBuilder();
                var errorBuilder = new StringBuilder();

                var process = Process.Start(processStartInfo);

                process.OutputDataReceived += (sender, e) => outputBuilder.AppendLine(e.Data);

                process.ErrorDataReceived += (sender, e) => errorBuilder.AppendLine(e.Data);

                process.BeginOutputReadLine();

                process.BeginErrorReadLine();

                await process.WaitForExitAsync();

                var exitCode = process.ExitCode;
                var output = outputBuilder.ToString();
                var error = errorBuilder.ToString();


                if (exitCode != 0)
                {
                    return StatusCode(500, $"Error occurred");
                }

                var containerId = Guid.NewGuid().ToString();
                var startContainerProcessStartInfo = new ProcessStartInfo
                {
                    FileName = "docker",
                    Arguments = $"run --rm --name {containerId} {fileId}:latest",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                var startContainerOutputBuilder = new StringBuilder();
                var startContainerErrorBuilder = new StringBuilder();

                var startContainerProcess = Process.Start(startContainerProcessStartInfo);
                startContainerProcess.OutputDataReceived += (sender, e) => startContainerOutputBuilder.AppendLine(e.Data);
                startContainerProcess.ErrorDataReceived += (sender, e) => startContainerErrorBuilder.AppendLine(e.Data);

                startContainerProcess.BeginOutputReadLine();
                startContainerProcess.BeginErrorReadLine();

                await startContainerProcess.WaitForExitAsync();

                var startContainerExitCode = process.ExitCode;
                var startContainerOutput = startContainerOutputBuilder.ToString();
                var startContainerError = startContainerErrorBuilder.ToString();

                if (startContainerExitCode != 0)
                {
                    return StatusCode(500, $"Error occurred during container execution: {startContainerError}");
                }

                System.IO.File.Delete(cppFilePath);
                System.IO.File.Delete(dockerfilePath);

                return Ok(startContainerOutput);
 
            } catch (Exception ex)
            {
                return BadRequest("Fail to execute code!");
            }
            finally
            {
                // remove docker image
                var processInfor = new ProcessStartInfo
                {
                    FileName = "docker",
                    Arguments = $"rmi {fileId}",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                };

                var process = Process.Start(processInfor);
                process?.WaitForExit();
            }
        }
    }
}
