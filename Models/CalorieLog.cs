namespace FitnessTracker.Api.Models
{
    public class CalorieLog
    {
        public string Username { get; set; } = string.Empty;
        public string Meal { get; set; } = string.Empty;
        public int Calories { get; set; }
        public DateTime Date { get; set; }
    }
}
