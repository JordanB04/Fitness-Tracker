using System;

namespace FitnessTracker.Api.Models
{
    public class CalorieHistoryItem
    {
        public DateTime Date { get; set; }
        public int TotalCaloriesConsumed { get; set; }
        public int TotalCaloriesBurned { get; set; }
    }
}
