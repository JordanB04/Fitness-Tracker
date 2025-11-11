using Microsoft.AspNetCore.Mvc;
using FitnessTracker.Api.Models;

namespace FitnessTracker.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CalorieController : ControllerBase
    {
        private static readonly List<CalorieLog> _logs = new();

        [HttpGet]
        public IActionResult GetAll() => Ok(_logs);

        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var log = _logs.FirstOrDefault(l => l.Id == id);
            return log == null ? NotFound() : Ok(log);
        }

        [HttpPost]
        public IActionResult Create([FromBody] CalorieLog log)
        {
            log.Id = _logs.Count + 1;
            _logs.Add(log);
            return CreatedAtAction(nameof(GetById), new { id = log.Id }, log);
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var log = _logs.FirstOrDefault(l => l.Id == id);
            if (log == null) return NotFound();
            _logs.Remove(log);
            return NoContent();
        }
    }
}
