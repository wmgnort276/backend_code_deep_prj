using backend.Repository;
using backend.RequestModel;
using backend.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RatingController : ControllerBase
    {
        private readonly IRatingRepository _ratingService;
        public RatingController(IRatingRepository ratingService) {
            _ratingService = ratingService;
        }

        [HttpPost]
        public IActionResult Rating(RatingModel rating)
        {
            try
            {
                var userId = (User.FindFirst(ClaimTypes.NameIdentifier)?.Value) 
                    ?? throw new Exception("Not found user!");
                var ratingRespone = _ratingService.CreateRating(userId, rating);

                return Ok(new ResponseModel.Response { Status = "200", Message = "Success", Data = ratingRespone});
            }
            catch (Exception ex)
            {
                return BadRequest("Rating fail");
            }
        }
    }
}
