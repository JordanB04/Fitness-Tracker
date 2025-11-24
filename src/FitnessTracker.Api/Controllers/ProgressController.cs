using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FitnessTracker.Api.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class ProgressController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            // Placeholder to keep the API compiling & running
            return Ok(new { message = "Progress tracking coming soon" });
        }
    }
}
