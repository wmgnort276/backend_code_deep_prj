using backend.Repository;
using backend.RequestModel;
using backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ExerciseTypeController : ControllerBase
    {
        private readonly IExerciseTypeRepository _service;

        public ExerciseTypeController(IExerciseTypeRepository exerciseTypeService) {
            _service = exerciseTypeService;
        }

        [HttpGet]
        public IActionResult Get()
        {
            try
            {
                return Ok( new ResponseModel.Response { 
                    Status = "200",
                    Message = "Success",
                    Data = _service.GetAll()
                });
            } catch(Exception ex)
            {
                return BadRequest("Failed!");
            }
        }

        [Authorize]
        [HttpPost]
        public IActionResult Add(ExerciseTypeModel model)
        {
            try
            {
               return Ok(_service.Add(model));
            } catch(Exception ex) {
                return BadRequest("Failed!");
             }
        }
    }
}
