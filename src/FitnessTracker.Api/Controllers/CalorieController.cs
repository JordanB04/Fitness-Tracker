using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
<<<<<<< HEAD
=======
using System.Text.Json;
>>>>>>> origin/main
using FitnessTracker.Api.Models;

namespace FitnessTracker.Api.Controllers
{
    [ApiController]
<<<<<<< HEAD
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
=======
    [Route("api/[controller]")]
    [Authorize]
    public class CalorieController : ControllerBase
    {
        private readonly string _dataPath = Path.Combine("data", "calories.json");

        [HttpPost("add")]
        public IActionResult AddCalorie([FromBody] CalorieLog log)
        {
            if (log == null || string.IsNullOrWhiteSpace(log.Meal))
                return BadRequest("Meal and calories are required.");

            var username = User.Identity?.Name ?? "unknown";
            log.Username = username;
            log.Date = DateTime.UtcNow.Date;

            var logs = LoadLogs();
            logs.Add(log);
            SaveLogs(logs);

            return Ok(new { message = "Calorie log added successfully." });
        }

        [HttpGet("summary")]
        public IActionResult GetSummary()
        {
            var username = User.Identity?.Name ?? "unknown";
            var logs = LoadLogs().Where(l => l.Username == username);

            var summary = logs
                .GroupBy(l => l.Date)
                .Select(g => new
                {
                    Date = g.Key,
                    TotalCalories = g.Sum(x => x.Calories)
                });

            return Ok(summary);
        }

        private List<CalorieLog> LoadLogs()
        {
            if (!System.IO.File.Exists(_dataPath))
                return new List<CalorieLog>();

            var json = System.IO.File.ReadAllText(_dataPath);
            return string.IsNullOrWhiteSpace(json)
                ? new List<CalorieLog>()
                : JsonSerializer.Deserialize<List<CalorieLog>>(json) ?? new List<CalorieLog>();
        }

        private void SaveLogs(List<CalorieLog> logs)
        {
            var json = JsonSerializer.Serialize(logs, new JsonSerializerOptions { WriteIndented = true });
            System.IO.File.WriteAllText(_dataPath, json);
>>>>>>> origin/main
        }
    }
}
