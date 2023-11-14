using backend.Data;
using backend.Repository;
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

        [HttpGet]
        public async Task<IActionResult> GetUserExercise() {
            var userName = User.FindFirst(ClaimTypes.Name)?.Value;
            var user = await userManager.FindByNameAsync(userName);
            var result = _userService.GetUserExercise(user.Id);
            return Ok(result);
        }
    }
}
