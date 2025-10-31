namespace FitnessTracker.Api.Models
{
    // Represents a registration request body
    public record RegisterRequest(string Username, string Password);

    // Represents a login request body
    public record LoginRequest(string Username, string Password);
}
