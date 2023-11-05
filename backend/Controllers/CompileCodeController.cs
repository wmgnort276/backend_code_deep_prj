using backend.Data;
using backend.RequestModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

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
            string filePath = Path.Combine(Directory.GetCurrentDirectory(), fileName + ".cpp"); // Đường dẫn đến tệp tin cpp
            string compiledFilePath = Path.Combine(Directory.GetCurrentDirectory(), fileName); // Đường dẫn đến tệp thực thi
            Console.WriteLine("File path:  " + compiledFilePath);
            try
            {
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

    }
}
