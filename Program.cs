using System.Security.Claims;
using System.Text;
using System.Text.Json;
using BCrypt.Net;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using FitnessTracker.Api.Models;

var builder = WebApplication.CreateBuilder(args);

// ------------------------------------------------------
// 1️⃣ Configure JWT + Swagger
// ------------------------------------------------------
var jwtKey = "SUPER_SECRET_KEY_CHANGE_THIS_64_CHAR_STRING_1234567890_ABCDEFGHIJKLMN";
var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = key
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "🏋️‍♂️ Fitness Tracker API",
        Version = "v1",
        Description = "Track calories, workouts, and daily summaries with JWT login/logout."
    });

    // JWT button in Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Enter 'Bearer {token}'",
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// ------------------------------------------------------
// 2️⃣ Enable Middleware
// ------------------------------------------------------
app.UseSwagger();
app.UseSwaggerUI();
app.UseAuthentication();
app.UseAuthorization();
app.UseHttpsRedirection();

// ------------------------------------------------------
// 3️⃣ Data Persistence Setup
// ------------------------------------------------------
var dataDir = Path.Combine(Directory.GetCurrentDirectory(), "data");
if (!Directory.Exists(dataDir))
    Directory.CreateDirectory(dataDir);

string calorieFile = Path.Combine(dataDir, "calories.json");
string exerciseFile = Path.Combine(dataDir, "exercises.json");
string summaryFile = Path.Combine(dataDir, "summaries.json");
string usersFile = Path.Combine(dataDir, "users.json");

// Deserialize if existing, otherwise create new
var calorieLogs = File.Exists(calorieFile)
    ? JsonSerializer.Deserialize<List<CalorieLog>>(File.ReadAllText(calorieFile)) ?? new()
    : new();

var exerciseLogs = File.Exists(exerciseFile)
    ? JsonSerializer.Deserialize<List<ExerciseLog>>(File.ReadAllText(exerciseFile)) ?? new()
    : new();

var summaries = File.Exists(summaryFile)
    ? JsonSerializer.Deserialize<List<DailySummary>>(File.ReadAllText(summaryFile)) ?? new()
    : new();

var users = File.Exists(usersFile)
    ? JsonSerializer.Deserialize<List<User>>(File.ReadAllText(usersFile)) ?? new()
    : new();

void SaveData<T>(string path, List<T> data)
{
    var json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
    File.WriteAllText(path, json);
}

// ------------------------------------------------------
// 4️⃣ Authentication Endpoints
// ------------------------------------------------------
app.MapPost("/register", (User user) =>
{
    if (users.Any(u => u.Username == user.Username))
        return Results.BadRequest(new { message = "Username already exists." });

    user.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);
    users.Add(user);
    SaveData(usersFile, users);

    return Results.Ok(new { message = "User registered successfully!" });
});

app.MapPost("/login", (User credentials) =>
{
    var user = users.FirstOrDefault(u => u.Username == credentials.Username);
    if (user == null || !BCrypt.Net.BCrypt.Verify(credentials.Password, user.Password))
        return Results.Unauthorized();

    var claims = new[]
    {
        new Claim(ClaimTypes.Name, user.Username)
    };

    var token = new System.IdentityModel.Tokens.Jwt.JwtSecurityToken(
        claims: claims,
        expires: DateTime.Now.AddHours(4),
        signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
    );

    string jwt = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler().WriteToken(token);

    return Results.Ok(new { token = jwt });
});

app.MapPost("/logout", () => Results.Ok(new { message = "Logged out successfully (client-side handled)" }));

// ------------------------------------------------------
// 5️⃣ Fitness Tracker Endpoints
// ------------------------------------------------------
app.MapPost("/calories", (CalorieLog log, ClaimsPrincipal user) =>
{
    if (user.Identity == null || !user.Identity.IsAuthenticated)
        return Results.Unauthorized();

    log.Date = DateTime.Now;
    log.Username = user.Identity.Name ?? "Unknown";
    calorieLogs.Add(log);
    SaveData(calorieFile, calorieLogs);

    return Results.Ok(new { message = "Calorie log saved successfully!", log });
}).RequireAuthorization();

app.MapPost("/exercises", (ExerciseLog log, ClaimsPrincipal user) =>
{
    if (user.Identity == null || !user.Identity.IsAuthenticated)
        return Results.Unauthorized();

    log.Date = DateTime.Now;
    log.Username = user.Identity.Name ?? "Unknown";
    exerciseLogs.Add(log);
    SaveData(exerciseFile, exerciseLogs);

    return Results.Ok(new { message = "Exercise log saved successfully!", log });
}).RequireAuthorization();

app.MapGet("/summary", (ClaimsPrincipal user) =>
{
    if (user.Identity == null || !user.Identity.IsAuthenticated)
        return Results.Unauthorized();

    string username = user.Identity.Name ?? "Unknown";
    var userCalories = calorieLogs.Where(c => c.Username == username);
    var userExercises = exerciseLogs.Where(e => e.Username == username);

    var summary = new DailySummary
    {
        Date = DateTime.Now.ToString("yyyy-MM-dd"),
        TotalCalories = userCalories.Sum(c => c.Calories),
        TotalMeals = userCalories.Count(),
        TotalDuration = userExercises.Sum(e => e.Duration),
        TotalWorkouts = userExercises.Count(),
        LastMeal = userCalories.LastOrDefault()?.MealName ?? "N/A",
        LastWorkout = userExercises.LastOrDefault()?.Workout ?? "N/A"
    };

    summaries.Add(summary);
    SaveData(summaryFile, summaries);

    return Results.Ok(summary);
}).RequireAuthorization();

// ------------------------------------------------------
// 6️⃣ Health Check
// ------------------------------------------------------
app.MapGet("/health", () =>
    Results.Ok(new { status = "ok", time = DateTime.Now }));

// ------------------------------------------------------
// 7️⃣ Frontend Hosting
// ------------------------------------------------------
app.UseDefaultFiles();  // looks for index.html
app.UseStaticFiles();   // serves HTML, CSS, JS from wwwroot

// ------------------------------------------------------
// 8️⃣ Run Application
// ------------------------------------------------------
await app.RunAsync("http://localhost:8080");
