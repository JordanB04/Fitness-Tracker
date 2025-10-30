using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FitnessTracker.Models;
using System.Text.Json;

namespace FitnessTracker.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CalorieController : ControllerBase
    {
        private static readonly string DataPath = Path.Combine("Data", "CalorieData.json");

        private List<CalorieLog> LoadLogs()
        {
            if (!System.IO.File.Exists(DataPath))
                return new List<CalorieLog>();

            var json = System.IO.File.ReadAllText(DataPath);
            return string.IsNullOrWhiteSpace(json) ? new List<CalorieLog>() :
                JsonSerializer.Deserialize<List<CalorieLog>>(json) ?? new List<CalorieLog>();
        }

        private void SaveLogs(List<CalorieLog> logs)
        {
            var json = JsonSerializer.Serialize(logs, new JsonSerializerOptions { WriteIndented = true });
            System.IO.File.WriteAllText(DataPath, json);
        }

        [HttpPost("add")]
        public IActionResult AddEntry(CalorieLog log)
        {
            var logs = LoadLogs();
            logs.Add(log);
            SaveLogs(logs);
            return Ok("Calorie log added successfully!");
        }

        [HttpGet("all")]
        public IActionResult GetAllEntries()
        {
            var logs = LoadLogs();
            return Ok(logs);
        }

        [HttpGet("total")]
        public IActionResult GetTotalCalories()
        {
            var logs = LoadLogs();
            var total = logs.Sum(c => c.Calories);
            return Ok(new { totalCalories = total });
        }
    }
}
