using backend.Repository;
using backend.ResponseModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SubmissionController : ControllerBase
    {
        private readonly ISubmissionRepository _services;

        public SubmissionController(ISubmissionRepository submissionRepository) { 
            _services = submissionRepository;
        }

        [Authorize]
        [HttpGet]
        public IActionResult GetUserSubmissions(Guid exerciseId) {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                return Ok(new Response
                {
                    Status = "200",
                    Message = "Success",
                    Data = _services.GetUserSubmission(userId, exerciseId)
                });
            } catch (Exception ex)
            {
                return BadRequest("Fail to get submission!");
            }
        }

        [Authorize]
        [HttpGet("{id}")]
        public IActionResult GetById(Guid id)
        {
            try
            {
                var submission = _services.GetSubmissionById(id);
                return Ok(new ResponseModel.Response { Status = "200", Message = "Success", Data = submission });
            }
            catch (Exception ex)
            {
                return BadRequest("Get fail");
            }
        }
    }
}
