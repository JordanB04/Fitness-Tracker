namespace FitnessTracker.Api.Models
{
    public class DailySummary
    {
        public string Date { get; set; } = string.Empty;
        public int TotalCalories { get; set; }
        public int TotalMeals { get; set; }
        public int TotalWorkouts { get; set; }
        public int TotalDuration { get; set; }
        public string LastMeal { get; set; } = string.Empty;
        public string LastWorkout { get; set; } = string.Empty;
    }
}
