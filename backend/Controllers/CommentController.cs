using backend.Repository;
using backend.RequestModel;
using backend.ResponseModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CommentController : ControllerBase
    {
        private readonly ICommentRepository _service;

        public CommentController(ICommentRepository repository) {
            _service = repository;
        }

        [HttpPost]
        public IActionResult CreateComment(CommentModel comment)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userId != null)
                {
                    comment.UserId = userId;
                }

                _service.CreateComment(comment);
                return Ok("Comment created");
            } catch (Exception ex)
            {
                Console.WriteLine(ex);
                return BadRequest("Error");
            }
        }

        [HttpGet]
        public IActionResult GetComments(Guid ExerciseId)
        {
            try
            {
                var result = _service.GetComments(ExerciseId);
                return Ok(new Response { Status = "200", Message = "Success", Data = result });
            } catch(Exception ex)
            {
                Console.WriteLine(ex);
                return BadRequest("Error");
            }
        }
    }
}
