using backend.Repository;
using backend.ResponseModel;
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
    }
}
