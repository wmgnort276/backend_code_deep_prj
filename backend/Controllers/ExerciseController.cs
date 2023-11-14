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
                var newExerciseModel = new ExerciseModel
                {
                    Name = model.Name,
                    Description = model.Description,
                    ExerciseLevelId = model.ExerciseLevelId,
                    ExerciseTypeId = model.ExerciseTypeId,
                    HintCode = model.HintCode,
                    TimeLimit = model.TimeLimit,
                };
                if (model.File != null && model.File.Length > 0)
                {
                    using var memoryStream = new MemoryStream();
                    model.File.CopyTo(memoryStream);
                    var fileData = memoryStream.ToArray();
                    return CreatedAtAction(nameof(Add), _services.Add(newExerciseModel, fileData));
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

        [HttpGet]
        public IActionResult Exercises()
        {
            try
            {
                return Ok(_services.All());
            } catch (Exception ex) {
                return BadRequest("Fail to get exercise!");
            }
        }

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
                    Data = result
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
                if (model?.File != null && model?.File?.Length > 0)
                {
                    using var memoryStream = new MemoryStream();
                    model.File.CopyTo(memoryStream);
                    var fileData = memoryStream.ToArray();
                    return Ok(new Response { Status = "200", Message = "Success" , Data = _services.Edit(model, fileData)});
                }
                else
                {
                    return Ok(new Response { Status = "200", Message = "Success", Data = _services.Edit(model, null) });
                }
            }
            catch (Exception ex)
            {
                return BadRequest("Create failed");
            }
        }
    }
}
