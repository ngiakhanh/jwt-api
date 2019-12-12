using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JWTAPI.Controllers
{
    [Route("/api/[controller]")]
    public class ProtectedController : Controller
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