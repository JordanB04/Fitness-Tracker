using System;

namespace FitnessTracker.Api.Models
{
    public class CalorieEntry
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string MealName { get; set; } = string.Empty;
        public int Calories { get; set; }
        public DateTime Date { get; set; }
    }
}
