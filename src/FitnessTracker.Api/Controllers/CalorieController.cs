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
    public class CaloriesController : ControllerBase
    {
        private int GetUserId()
        {
            var sub = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(sub, out var id))
            {
                throw new InvalidOperationException("Invalid user id in token");
            }
            return id;
        }

        [HttpPost("log")]
        public IActionResult LogCalories([FromBody] CalorieLogRequest req)
        {
            var entry = new CalorieEntry
            {
                Id = FakeDatabase.NextCalorieId(),
                UserId = GetUserId(),
                MealName = req.MealName,
                Calories = req.Calories,
                Date = req.Date
            };

            FakeDatabase.Calories.Add(entry);
            return Ok(entry);
        }

        [HttpGet("history")]
        public IActionResult GetHistory(DateTime? startDate, DateTime? endDate)
        {
            int userId = GetUserId();

            var from = startDate?.Date ?? DateTime.UtcNow.AddDays(-30).Date;
            var to = endDate?.Date ?? DateTime.UtcNow.Date;

            var calories = FakeDatabase.Calories
                .Where(c => c.UserId == userId && c.Date.Date >= from && c.Date.Date <= to)
                .ToList();

            var exercises = FakeDatabase.Exercises
                .Where(e => e.UserId == userId && e.Date.Date >= from && e.Date.Date <= to)
                .ToList();

            var result = calories
                .GroupBy(c => c.Date.Date)
                .Select(day => new CalorieHistoryItem
                {
                    Date = day.Key,
                    TotalCaloriesConsumed = day.Sum(x => x.Calories),
                    TotalCaloriesBurned = exercises
                        .Where(e => e.Date.Date == day.Key)
                        .Sum(e => e.CaloriesBurned)
                })
                .OrderBy(x => x.Date)
                .ToList();

            return Ok(result);
        }
    }
}
