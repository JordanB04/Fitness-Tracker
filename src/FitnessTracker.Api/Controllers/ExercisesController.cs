using System;
using System.Linq;
using System.Security.Claims;
using FitnessTracker.Api.Data;
using FitnessTracker.Api.Dtos;
using FitnessTracker.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FitnessTracker.Api.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class ExercisesController : ControllerBase
    {
        private int GetUserId()
        {
            var sub = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int.TryParse(sub, out var id);
            return id;
        }

        [HttpPost("log")]
        public IActionResult LogExercise([FromBody] ExerciseLogRequest req)
        {
            var entry = new ExerciseEntry
            {
                Id = FakeDatabase.NextExerciseId(),
                UserId = GetUserId(),
                ExerciseName = req.ExerciseName,
                CaloriesBurned = req.CaloriesBurned,
                Date = req.Date
            };

            FakeDatabase.Exercises.Add(entry);
            return Ok(entry);
        }

        // Simple suggestion endpoint
        [HttpGet("suggest")]
        public IActionResult Suggest()
        {
            int userId = GetUserId();
            var totalCaloriesToday = FakeDatabase.Calories
                .Where(c => c.UserId == userId && c.Date.Date == DateTime.UtcNow.Date)
                .Sum(c => c.Calories);

            var suggestion = new ExerciseSuggestion
            {
                ExerciseType = "walking",
                DurationMinutes = 30,
                EstimatedCaloriesBurned = 150,
                Note = totalCaloriesToday > 2000
                    ? "You’ve eaten over 2000 calories today. Try a 30-minute walk."
                    : "Keep moving! A 30-minute walk is a good baseline."
            };

            return Ok(suggestion);
        }
    }
}
