namespace FitnessTracker.Api.Models
{
    public class WorkLog
    {
        public string Id { get; set; } = default!;
        public string UserId { get; set; } = default!;
        public string Title { get; set; } = default!;
        public string Category { get; set; } = "General";
        public int Minutes { get; set; }
        public string? Notes { get; set; }
        public DateTime Date { get; set; } // stored as UTC Date
    }

    public record WorkLogCreate(string Title, int Minutes, string? Category, string? Notes, DateTime? Date);
    public record WorkLogUpdate(string? Title, int? Minutes, string? Category, string? Notes, DateTime? Date);
}
