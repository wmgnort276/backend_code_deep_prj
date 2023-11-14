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
            string fileName = "example12"; // Tạo tên tệp tin ngẫu nhiên để tránh xung đột tên tệp tin
            string filePath = Path.Combine(Directory.GetCurrentDirectory() + "\\Sandbox", fileName + ".cpp"); // Đường dẫn đến tệp tin cpp
            string compiledFilePath = Path.Combine(Directory.GetCurrentDirectory() + "\\Sandbox", fileName); // Đường dẫn đến tệp thực thi
            Console.WriteLine("File path:  " + compiledFilePath);
            try
            {
                // config docker
                // var client = new DockerClientConfiguration().CreateClient();
                // var containerConfig = new CreateContainerParameters
                //{
                //    Image = "my-running-image",
                //    Name = "my-container-name",
                //    // Add any additional configuration options here
                //};

                //var containerCreateResponse = client.Containers.CreateContainerAsync(containerConfig).Result;

                //var containerStartResponse = client.Containers.StartContainerAsync(containerCreateResponse.ID, 
                //    new ContainerStartParameters()).Result;

                //if (containerStartResponse)
                //{
                //    var logsStream = client.Containers.GetContainerLogsAsync(containerCreateResponse.ID, 
                //        new ContainerLogsParameters { ShowStdout = true, ShowStderr = true }).Result;

                //    using (var reader = new StreamReader(logsStream, Encoding.UTF8))
                //    {
                //        StringBuilder logsBuilder = new StringBuilder();
                //        byte[] buffer = new byte[4096];
                //        int bytesRead;
                //        while ((bytesRead = reader.BaseStream.Read(buffer, 0, buffer.Length)) > 0)
                //        {
                //            string logs = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                //            logsBuilder.Append(logs);
                //        }

                //        string logsResult = logsBuilder.ToString();
                //        Console.WriteLine("Container logs:\n" + logsResult);
                //    }
                //}

                // Ghi mã nguồn C++ vào tệp tin
                System.IO.File.WriteAllText(filePath, cppCode);

                // Biên dịch tệp tin cpp thành tệp thực thi
                string compilerPath = "g++"; // Đường dẫn đến trình biên dịch g++
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

                // Thực thi tệp thực thi và nhận kết quả
                string executionResult = string.Empty;

                ProcessStartInfo executionProcessStartInfo = new ProcessStartInfo
                {
                    FileName = compiledFilePath,
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                //using (Process executionProcess = new Process())
                //{
                //    executionProcess.StartInfo = executionProcessStartInfo;
                //    executionProcess.Start();
                //    executionResult = executionProcess.StandardOutput.ReadToEnd();
                //    executionProcess.WaitForExit();
                //}

                using (Process executionProcess = new Process())
                {
                    executionProcess.StartInfo = executionProcessStartInfo;
                    // Use task to run the execution process
                    var task = Task.Run(() =>
                    {
                        executionProcess.Start();
                        executionResult = executionProcess.StandardOutput.ReadToEnd();
                        // executionProcess.WaitForExit(); => no need to do not wait the process
                    });

                    var completed = task.Wait(TimeSpan.FromSeconds(5));

                    if (!completed)
                    {
                        // Nếu quá thời gian, throw ra ngoại lệ và xử lý tương ứng
                        throw new TimeoutException("Execution timed out.");
                    }
                }

                    return Ok(executionResult);
            }
            catch (Exception ex)
            {
                return BadRequest("Compile fail!");
            }
            finally
            {
                // Xóa các tệp tin tạm thời
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

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            // Currently using as image-tag
            var fileId = userId;
            try
            {
                var sourceCode = example.Code;
                string cppFilePath = Path.Combine(Directory.GetCurrentDirectory() + "\\Sandbox", fileId + ".cpp");
                await System.IO.File.WriteAllTextAsync(cppFilePath, sourceCode);

                // Add file cpp to docker image for building
                var dockerfileContent = string.Format(DockerfileTemplate, $"{fileId}.cpp");

                // Create file path for docker file
                var dockerfilePath = Path.Combine(Directory.GetCurrentDirectory() + "\\Sandbox", "Dockerfile");
                await System.IO.File.WriteAllTextAsync(dockerfilePath, dockerfileContent);

                var testDomain = Path.Combine(Directory.GetCurrentDirectory() + "\\Sandbox");

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
