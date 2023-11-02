using backend.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BaseController : ControllerBase
    {
        //[Authorize(Roles = UserRoles.Admin)]
        [HttpGet]
        public IActionResult GetExample()
        {
            var response = new List<string>();
            response.Add("hello");
            response.Add("hello1");
            response.Add("hello2");
            response.Add("hello3");
            return Ok(response);
        }
    }
}
