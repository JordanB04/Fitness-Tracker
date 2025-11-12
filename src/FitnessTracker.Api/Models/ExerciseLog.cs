namespace FitnessTracker.Api.Models
{
    public class ExerciseLog
    {
        public string Workout { get; set; } = string.Empty;
        public int Duration { get; set; }  // in minutes
        public DateTime Date { get; set; } = DateTime.Now;
    }
}
