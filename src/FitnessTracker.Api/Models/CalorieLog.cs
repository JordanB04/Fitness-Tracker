using System;

namespace FitnessTracker.Api.Models
{
    public class CalorieLog
    {
        public int Id { get; set; }  // ✅ Fixes "Id not found" build errors
        public DateTime Date { get; set; }
        public string Meal { get; set; } = string.Empty;
        public int Calories { get; set; }
    }
}
