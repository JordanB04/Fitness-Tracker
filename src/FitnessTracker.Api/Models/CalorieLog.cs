namespace FitnessTracker.Api.Models
{
    public class CalorieLog
    {
        public string MealName { get; set; } = string.Empty;
        public int Calories { get; set; }
        public DateTime Date { get; set; } = DateTime.Now;
    }
}
