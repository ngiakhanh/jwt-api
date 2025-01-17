using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JWTAPI.Controllers
{
    [Route("/api/[controller]")]
    [ApiController]
    public class ProtectedController : ControllerBase
    {
        [HttpGet]
        [Authorize]
        [Route("commonusers")]
        public IActionResult GetProtectedData()
        {
            return Ok("Hello world from protected controller.");
        }

        [HttpGet]
        [Authorize(Roles = "Administrator")]
        [Route("administrators")]
        public IActionResult GetProtectedDataForAdmin()
        {
            return Ok("Hello admin!");
        }
    }
}