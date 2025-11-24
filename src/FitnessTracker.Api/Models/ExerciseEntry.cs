using System;

namespace FitnessTracker.Api.Models
{
    public class ExerciseEntry
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string ExerciseName { get; set; } = string.Empty;
        public int CaloriesBurned { get; set; }
        public DateTime Date { get; set; }
    }
}
