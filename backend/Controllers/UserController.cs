using backend.Data;
using backend.Repository;
using backend.ResponseModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserRepository _userService;
        private readonly UserManager<Users> userManager;

        public UserController(IUserRepository repository, UserManager<Users> userManager) {
            _userService = repository;
            this.userManager = userManager;
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetUserExercises() {
            try
            {
                var userName = User.FindFirst(ClaimTypes.Name)?.Value;
                var user = await userManager.FindByNameAsync(userName);
                var result = _userService.GetUserResolveExercises(user.Id);
                return Ok(new Response
                {
                    Status = "200",
                    Message = "Success",
                    Data = result
                });
            } catch (Exception ex)
            {
                return BadRequest("Failed");
            }
        }

        [Authorize]
        [HttpGet("all")]
        public async Task<IActionResult> GetUserAllExercises()
        {
            try
            {
                var userName = User.FindFirst(ClaimTypes.Name)?.Value;
                var user = await userManager.FindByNameAsync(userName);
                var result = _userService.GetUserSubmitExercises(user.Id);
                return Ok(new Response
                {
                    Status = "200",
                    Message = "Success",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                return BadRequest("Failed");
            }
        }


        [HttpGet("rank")]
        public IActionResult GetListRanking()
        {
            try
            {
                var result = _userService.GetRankingList();
                return Ok(new Response
                {
                    Status = "200",
                    Message = "Success",
                    Data = result
                });
            } catch (Exception ex)
            {
                Console.WriteLine("GetListRanking error: " + ex.Message);
                return BadRequest("Failed");
            }
        }

        [HttpGet("user-rank")]
        public async Task<IActionResult> GetUserRanking()
        {
            try
            {
                var userName = User.FindFirst(ClaimTypes.Name)?.Value;
                var user = await userManager.FindByNameAsync(userName);

                var result = _userService.GetUserRanking(user.Id);
                return Ok(new Response
                {
                    Status = "200",
                    Message = "Success",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine("GetListRanking error: " + ex.Message);
                return BadRequest("Failed");
            }
        }
    }
}
