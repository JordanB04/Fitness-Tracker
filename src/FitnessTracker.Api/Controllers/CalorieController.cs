using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FitnessTracker.Api.Models;

namespace FitnessTracker.Api.Controllers
{
    [ApiController]
    [Route("calories")]
    [Authorize]
    public class CalorieController : ControllerBase
    {
        private static readonly List<CalorieLog> Logs = new();

        [HttpPost("add")]
        public IActionResult AddLog([FromBody] CalorieLog log)
        {
            log.Date = DateTime.Now;
            Logs.Add(log);
            return Ok(new { message = "Log added successfully", log });
        }

        [HttpGet("list")]
        public IActionResult GetLogs(string username)
        {
            var userLogs = Logs.Where(l => l.Username == username).ToList();
            return Ok(userLogs);
        }

        [HttpGet("summary")]
        public IActionResult GetSummary(string username)
        {
            var totalCalories = Logs.Where(l => l.Username == username).Sum(l => l.Calories);
            return Ok(new { username, totalCalories });
        }
    }
}
