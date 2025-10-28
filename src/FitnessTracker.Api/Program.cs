using Microsoft.OpenApi.Models;
using FitnessTracker.Api.Models;
using System.Text.Json;
using System.Diagnostics;

var builder = WebApplication.CreateBuilder(args);

// ----------------------
// 1️⃣ Configure Services
// ----------------------
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Jordan Fitness API",
        Version = "v1",
        Description = "A RESTful API for tracking calories, workouts, and daily summaries with JSON file persistence."
    });
});

var app = builder.Build();

// ----------------------
// 2️⃣ Enable Swagger Always
// ----------------------
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Jordan Fitness API v1");
    c.RoutePrefix = string.Empty; // ✅ Swagger loads at root (http://localhost:8080)
});

// ----------------------
// 3️⃣ Middleware
// ----------------------
app.UseHttpsRedirection();

// ----------------------
// 4️⃣ Persistent Data Setup
// ----------------------
var dataDir = Path.Combine(Directory.GetCurrentDirectory(), "data");
if (!Directory.Exists(dataDir)) Directory.CreateDirectory(dataDir);

var calorieFile = Path.Combine(dataDir, "calorieLogs.json");
var exerciseFile = Path.Combine(dataDir, "exerciseLogs.json");
var summaryFile = Path.Combine(dataDir, "summaries.json");

var calorieLogs = File.Exists(calorieFile)
    ? JsonSerializer.Deserialize<List<CalorieLog>>(File.ReadAllText(calorieFile)) ?? new()
    : new List<CalorieLog>();

var exerciseLogs = File.Exists(exerciseFile)
    ? JsonSerializer.Deserialize<List<ExerciseLog>>(File.ReadAllText(exerciseFile)) ?? new()
    : new List<ExerciseLog>();

var summaries = File.Exists(summaryFile)
    ? JsonSerializer.Deserialize<List<DailySummary>>(File.ReadAllText(summaryFile)) ?? new()
    : new List<DailySummary>();

void SaveData<T>(string path, List<T> data)
{
    var json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
    File.WriteAllText(path, json);
}

// ----------------------
// 5️⃣ API Endpoints
// ----------------------

// ✅ Health Check
app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

// ✅ Add Calorie Log
app.MapPost("/calories", (CalorieLog log) =>
{
    if (string.IsNullOrWhiteSpace(log.MealName))
        return Results.BadRequest(new { message = "Meal name is required." });

    if (log.Calories <= 0)
        return Results.BadRequest(new { message = "Calories must be greater than 0." });

    log.Date = DateTime.Now;
    calorieLogs.Add(log);
    SaveData(calorieFile, calorieLogs);

    return Results.Created("/calories", new { message = "Calorie log added successfully!", log });
});

// ✅ View All Calorie Logs
app.MapGet("/calories", () =>
{
    if (!calorieLogs.Any())
        return Results.Ok(new { message = "No calorie logs yet." });

    return Results.Ok(calorieLogs);
});

// ✅ Add Exercise Log
app.MapPost("/exercises", (ExerciseLog log) =>
{
    if (string.IsNullOrWhiteSpace(log.Workout))
        return Results.BadRequest(new { message = "Workout name is required." });

    if (log.Duration <= 0)
        return Results.BadRequest(new { message = "Duration must be greater than 0." });

    log.Date = DateTime.Now;
    exerciseLogs.Add(log);
    SaveData(exerciseFile, exerciseLogs);

    return Results.Created("/exercises", new { message = "Exercise log added successfully!", log });
});

// ✅ View All Exercise Logs
app.MapGet("/exercises", () =>
{
    if (!exerciseLogs.Any())
        return Results.Ok(new { message = "No exercise logs yet." });

    return Results.Ok(exerciseLogs);
});

// ✅ Combined Daily Summary
app.MapGet("/summary", () =>
{
    if (!calorieLogs.Any() && !exerciseLogs.Any())
        return Results.Ok(new { message = "No logs to summarize." });

    var today = DateTime.Now.ToString("yyyy-MM-dd");
    var totalCalories = calorieLogs.Sum(c => c.Calories);
    var totalMeals = calorieLogs.Count;
    var totalDuration = exerciseLogs.Sum(e => e.Duration);
    var totalWorkouts = exerciseLogs.Count;

    var summary = new DailySummary
    {
        Date = today,
        TotalCalories = totalCalories,
        TotalMeals = totalMeals,
        TotalDuration = totalDuration,
        TotalWorkouts = totalWorkouts,
        LastMeal = calorieLogs.LastOrDefault()?.MealName ?? "N/A",
        LastWorkout = exerciseLogs.LastOrDefault()?.Workout ?? "N/A"
    };

    var existing = summaries.FirstOrDefault(s => s.Date == today);
    if (existing == null)
        summaries.Add(summary);
    else
    {
        existing.TotalCalories = totalCalories;
        existing.TotalMeals = totalMeals;
        existing.TotalDuration = totalDuration;
        existing.TotalWorkouts = totalWorkouts;
        existing.LastMeal = summary.LastMeal;
        existing.LastWorkout = summary.LastWorkout;
    }

    SaveData(summaryFile, summaries);

    return Results.Ok(new { message = "Daily summary updated successfully.", summary });
});

// ✅ View Summary History
app.MapGet("/summary/history", () =>
{
    if (!summaries.Any())
        return Results.Ok(new { message = "No summaries available." });

    return Results.Ok(summaries);
});

// ✅ View Summary by Date
app.MapGet("/summary/{date}", (string date) =>
{
    var result = summaries.FirstOrDefault(s => s.Date == date);
    if (result == null)
        return Results.NotFound(new { message = $"No summary found for {date}." });

    return Results.Ok(result);
});

// ✅ Root Redirect
app.MapGet("/", () => Results.Redirect("/swagger"));

// ----------------------
// 6️⃣ Run App + Auto Open Swagger
// ----------------------
var url = "http://localhost:8080";
_ = Task.Run(async () =>
{
    await Task.Delay(1500); // wait for server to start
    try { Process.Start(new ProcessStartInfo { FileName = url, UseShellExecute = true }); } catch { }
});

await app.RunAsync(url);
