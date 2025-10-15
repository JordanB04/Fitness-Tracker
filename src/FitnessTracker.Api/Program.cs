using Microsoft.OpenApi.Models;

// Fitness Tracker API minimal host.
//
// This file contains the minimal WebApplication host used by the sample API. It registers
// a few simple endpoints (health, auth placeholders, logging placeholders, progress and suggestions)
// primarily for demonstration and tests. Many endpoints are left as TODO placeholders and
// should be implemented with proper authentication, validation and persistence in a full app.
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Fitness Tracker API", Version = "v1" });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// --- Health Check ---
// Simple health check endpoint that returns { status = "ok" } when the service is reachable.
app.MapGet("/health", () => Results.Ok(new { status = "ok" })).WithName("HealthCheck");

// --- User Authentication (placeholders) ---
// In the application, these would involve secure user management and JWT tokens.
app.MapPost("/auth/register", () => Results.Created("/auth/register", new { message = "TODO: create user" }));
app.MapPost("/auth/login", () => Results.Ok(new { token = "TODO: jwt" }));
app.MapPost("/auth/logout", () => Results.NoContent());

// --- User Profile (placeholders) ---
// In the app, these would interact with a database.
app.MapPost("/calories/log", () => Results.Created("/calories/log", new { message = "TODO: save intake" }));
app.MapPost("/exercises/log", () => Results.Created("/exercises/log", new { message = "TODO: save exercise" }));

// --- Progress Tracking (placeholders) ---
// In the app, these would compute actual progress based on logged data.
app.MapGet("/progress/daily", () => Results.Ok(new { totalConsumed = 0, totalBurned = 0 }));
app.MapGet("/progress/weekly", () => Results.Ok(new { }));
app.MapGet("/progress/monthly", () => Results.Ok(new { }));

// --- Suggestions (placeholders) ---
// In the app, these would analyze user data to provide suggestions, e.g., workout plans.
app.MapPost("/suggestions", () => Results.Ok(new { plan = "TODO: compute suggestions" }));

app.Run();

/// <summary>
/// Partial Program class to make the implicit <c>Program</c> class discoverable by integration tests.
/// </summary>
public partial class Program { }
