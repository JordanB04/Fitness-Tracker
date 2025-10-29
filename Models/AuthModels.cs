namespace FitnessTracker.Api.Models;

public record RegisterRequest(string Username, string Password);
public record LoginRequest(string Username, string Password);

public class User
{
    public string Id { get; set; } = default!;
    public string Username { get; set; } = default!;
    public string PasswordHash { get; set; } = default!;
}
