using System;

namespace FitnessTracker.Api.Dtos
{
    public class CalorieLogRequest
    {
        public string MealName { get; set; } = string.Empty;
        public int Calories { get; set; }
        public DateTime Date { get; set; }
    }
}
