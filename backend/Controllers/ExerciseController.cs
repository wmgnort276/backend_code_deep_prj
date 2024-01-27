using backend.Data;
using backend.Repository;
using backend.RequestModel;
using backend.ResponseModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Security.Claims;

namespace backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ExerciseController : ControllerBase
    {
        private readonly IExerciseRepository _services;
        public ExerciseController(IExerciseRepository repository) {
            _services = repository;
        }

        [Authorize(Roles = UserRoles.Admin)]
        [HttpPost]
        public IActionResult Add([FromForm] ExerciseModel model)
        {
            
            try
            {
                if (model.File != null && model.File.Length > 0 && model.FileJava != null && model.FileJava.Length > 0)
                {
                    using var memoryStream = new MemoryStream();
                    model.File.CopyTo(memoryStream);
                    var fileData = memoryStream.ToArray();

                    using var memoryStreamJava = new MemoryStream();
                    model.FileJava.CopyTo(memoryStreamJava);
                    var fileDataJava = memoryStreamJava.ToArray();

                    using var memoryStreamTestFile = new MemoryStream();
                    model.TestFile!.CopyTo(memoryStreamTestFile);
                    var testFile = memoryStreamTestFile.ToArray();

                    using var memoryStreamTestFileJava = new MemoryStream();
                    model.TestFileJava!.CopyTo(memoryStreamTestFileJava);
                    var testFileJava = memoryStreamTestFileJava.ToArray();

                    return CreatedAtAction(nameof(Add), _services.Add(model, fileData, fileDataJava, testFile, testFileJava));
                }
                else
                {
                    return BadRequest("File data not be null");
                }
            }
            catch (Exception ex)
            {
                return BadRequest("Create failed");
            }
        }

        [Authorize]
        [HttpGet("{id}")]
        public IActionResult GetById(Guid id)
        {   
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                return Ok(new ResponseModel.Response { Status = "200", Message = "Success", Data = _services.GetById(id, userId) });
            } catch (Exception ex)
            {
                return BadRequest("Get fail");
            }
        }

        [Authorize]
        [HttpGet("dowload")]
        public IActionResult DownloadFile(Guid id)
        {
            try
            {
                var fileContent = _services.DownLoadrunFille(id);
                if(fileContent != null)
                {
                    return File(fileContent, "application/octet-stream", "file_name.txt");
                }
                return BadRequest("Download failed!");
            } catch(Exception ex)
            {
                return BadRequest("Download failed!");
            }
        }

        [Authorize]
        [HttpGet]
        public IActionResult Exercises(int? exerciseLevelId, int? exerciseTypeId, string? keyword, int? pageIndex, int? pageSize)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                return Ok(_services.All(userId ?? "", exerciseLevelId, exerciseTypeId, keyword, pageIndex, pageSize));
            } catch (Exception ex) {
                return BadRequest("Fail to get exercise!");
            }
        }

        [Authorize]
        [HttpPost("submit")]
        public IActionResult SubmitCode(Guid id, SourceCode sourceCode)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var result = _services.Submit(id, userId, sourceCode);

                return Ok(new Response
                {
                    Status = "200",
                    Message = result == "1" ? "Success" : "Fail",
                    Data = result == "1" ? "Success" : result
                });
            }
            catch (Exception ex)
            {
                return BadRequest("Fail to get exercise!");
            }
        }

        [Authorize(Roles = UserRoles.Admin)]
        [HttpPost("edit")]
        public IActionResult Edit([FromForm] ExerciseModel model)
        {
            try
            {
                var fileData = new byte[0];
                var fileDataJava = new byte[0];
                var testFile = new byte[0];
                var testFileJava = new byte[0];

                if (model.File != null && model.File.Length > 0)
                {
                    using var memoryStream = new MemoryStream();
                    model.File.CopyTo(memoryStream);
                    fileData = memoryStream.ToArray();
                }

                if (model.FileJava != null && model.FileJava.Length > 0)
                {
                    using var memoryStreamJava = new MemoryStream();
                    model.FileJava.CopyTo(memoryStreamJava);
                    fileDataJava = memoryStreamJava.ToArray();
                }

                if (model.TestFile != null && model.TestFile.Length > 0)
                {
                    using var memoryStreamTestFile = new MemoryStream();
                    model.TestFile!.CopyTo(memoryStreamTestFile);
                    testFile = memoryStreamTestFile.ToArray();
                }

                if (model.TestFileJava != null && model.TestFileJava.Length > 0)
                {
                    using var memoryStreamTestFileJava = new MemoryStream();
                    model.TestFileJava!.CopyTo(memoryStreamTestFileJava);
                    testFileJava = memoryStreamTestFileJava.ToArray();
                }
                
                return Ok(new Response { Status = "200", Message = "Success" , Data = _services.Edit(model, fileData, fileDataJava, testFile, testFileJava) });
                
            }
            catch (Exception ex)
            {
                return BadRequest("Create failed" + ex.Message);
            }
        }

        [Authorize]
        [HttpPost("test-case")]
        public IActionResult RunTestCase(Guid id, SourceCode sourceCode) 
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var result = _services.CheckTestCase(id, sourceCode);

                return Ok(new Response
                {
                    Status = "200",
                    Message = "Success",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                return BadRequest("Fail to run test case!");
            }
        }


        [Authorize(Roles = UserRoles.Admin)]
        [HttpGet("admin")]
        public IActionResult ExercisesByAdmin(int? exerciseLevelId, int? exerciseTypeId, string? keyword, int? pageIndex, int? pageSize)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                return Ok(_services.AllForAdmin(userId ?? "", exerciseLevelId, exerciseTypeId, keyword, pageIndex, pageSize));
            }
            catch (Exception ex)
            {
                Console.WriteLine("Get exercise for admin fail: " + ex.Message);
                return BadRequest("Fail to get exercise!");
            }
        }
    }
}
