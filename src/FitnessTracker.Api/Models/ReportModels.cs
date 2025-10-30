namespace FitnessTracker.Models;


public class DailyReport
{
    public string Date { get; set; } = default!;
    public int TotalMinutes { get; set; }
    public Dictionary<string, int> ByCategory { get; set; } = new();
    public List<WorkLog> Entries { get; set; } = new();
}
