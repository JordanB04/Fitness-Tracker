using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using BCrypt.Net;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using FitnessTracker.Api.Models;

var builder = WebApplication.CreateBuilder(args);

// ----------------------
// 1) Auth + Swagger
// ----------------------
var jwtOptions = new JwtOptions(); // simple in-file config; change Secret in production
builder.Configuration.Bind("Jwt", jwtOptions);
jwtOptions.Secret ??= "CHANGE_ME_TO_A_LONG_RANDOM_SECRET_at_least_64_chars";

var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Secret));

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = key,
            ClockSkew = TimeSpan.FromSeconds(10)
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Fitness Tracker API",
        Version = "v1",
        Description = "Auth, Work tracking, JSON persistence, and reporting."
    });

    // Bearer support in Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        { new OpenApiSecurityScheme{ Reference = new OpenApiReference{ Type = ReferenceType.SecurityScheme, Id = "Bearer"}}, Array.Empty<string>() }
    });
});

var app = builder.Build();

// ----------------------
// 2) Swagger at root
// ----------------------
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Fitness Tracker API v1");
    c.RoutePrefix = string.Empty; // Swagger on "/"
});

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

// ----------------------
// 3) Persistence setup
// ----------------------
var dataDir = Path.Combine(Directory.GetCurrentDirectory(), "data");
Directory.CreateDirectory(dataDir);

var usersFile = Path.Combine(dataDir, "users.json");
var workFile  = Path.Combine(dataDir, "worklogs.json");
var blacklistFile = Path.Combine(dataDir, "token_blacklist.json");

// Load data
var users = Load<List<User>>(usersFile) ?? new List<User>();
var workLogs = Load<List<WorkLog>>(workFile) ?? new List<WorkLog>();
var blacklistedJtis = Load<HashSet<string>>(blacklistFile) ?? new HashSet<string>();

// Helpers
static T? Load<T>(string file) => File.Exists(file)
    ? JsonSerializer.Deserialize<T>(File.ReadAllText(file))
    : default;

static void Save<T>(string file, T data)
{
    var json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
    File.WriteAllText(file, json);
}

string NewId() => Guid.NewGuid().ToString("N");

// ----------------------
// 4) Health
// ----------------------
app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

// ----------------------
// 5) Auth: register, login, logout
// ----------------------
app.MapPost("/auth/register", (RegisterRequest req) =>
{
    if (string.IsNullOrWhiteSpace(req.Username) || string.IsNullOrWhiteSpace(req.Password))
        return Results.BadRequest(new { message = "Username and password are required." });

    if (users.Any(u => u.Username.Equals(req.Username, StringComparison.OrdinalIgnoreCase)))
        return Results.Conflict(new { message = "Username already exists." });

    var user = new User
    {
        Id = NewId(),
        Username = req.Username.Trim(),
        PasswordHash = BCrypt.Net.BCrypt.HashPassword(req.Password)
    };
    users.Add(user);
    Save(usersFile, users);
    return Results.Created($"/users/{user.Id}", new { message = "Registered.", user = new { user.Id, user.Username }});
});

app.MapPost("/auth/login", (LoginRequest req) =>
{
    var user = users.FirstOrDefault(u => u.Username.Equals(req.Username.Trim(), StringComparison.OrdinalIgnoreCase));
    if (user is null || !BCrypt.Net.BCrypt.Verify(req.Password, user.PasswordHash))
        return Results.Unauthorized();

    var jti = Guid.NewGuid().ToString();
    var claims = new[]
    {
        new Claim(JwtRegisteredClaimNames.Sub, user.Id),
        new Claim(JwtRegisteredClaimNames.UniqueName, user.Username),
        new Claim(JwtRegisteredClaimNames.Jti, jti)
    };

    var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
    var token = new JwtSecurityToken(
        claims: claims,
        notBefore: DateTime.UtcNow,
        expires: DateTime.UtcNow.AddHours(6),
        signingCredentials: creds);

    var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
    return Results.Ok(new { token = tokenString, expiresInHours = 6 });
});

app.MapPost("/auth/logout", (ClaimsPrincipal user, HttpContext ctx) =>
{
    // Blacklist current token jti
    var jti = user.FindFirstValue(JwtRegisteredClaimNames.Jti);
    if (string.IsNullOrEmpty(jti))
        return Results.BadRequest(new { message = "No active token." });

    blacklistedJtis.Add(jti);
    Save(blacklistFile, blacklistedJtis);
    return Results.Ok(new { message = "Logged out (token invalidated)." });
}).RequireAuthorization();

// Middleware to reject blacklisted tokens
app.Use(async (context, next) =>
{
    if (context.User?.Identity?.IsAuthenticated == true)
    {
        var jti = context.User.FindFirstValue(JwtRegisteredClaimNames.Jti);
        if (!string.IsNullOrEmpty(jti) && blacklistedJtis.Contains(jti))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsJsonAsync(new { message = "Token is revoked." });
            return;
        }
    }
    await next();
});

// ----------------------
// 6) Work tracking CRUD
// ----------------------
app.MapPost("/work", (ClaimsPrincipal principal, WorkLogCreate req) =>
{
    if (string.IsNullOrWhiteSpace(req.Title) || req.Minutes <= 0)
        return Results.BadRequest(new { message = "Title and positive minutes are required." });

    var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier) ?? principal.FindFirstValue(JwtRegisteredClaimNames.Sub)!;
    var wl = new WorkLog
    {
        Id = NewId(),
        UserId = userId,
        Title = req.Title.Trim(),
        Category = string.IsNullOrWhiteSpace(req.Category) ? "General" : req.Category.Trim(),
        Minutes = req.Minutes,
        Notes = req.Notes?.Trim(),
        Date = (req.Date ?? DateTime.UtcNow).Date
    };

    workLogs.Add(wl);
    Save(workFile, workLogs);
    return Results.Created($"/work/{wl.Id}", wl);
}).RequireAuthorization();

app.MapGet("/work", (ClaimsPrincipal principal) =>
{
    var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier) ?? principal.FindFirstValue(JwtRegisteredClaimNames.Sub)!;
    var mine = workLogs.Where(w => w.UserId == userId).OrderByDescending(w => w.Date).ThenByDescending(w => w.Id);
    return Results.Ok(mine);
}).RequireAuthorization();

app.MapGet("/work/{id}", (ClaimsPrincipal principal, string id) =>
{
    var userId = principal.FindFirstValue(JwtRegisteredClaimNames.Sub)!;
    var wl = workLogs.FirstOrDefault(w => w.Id == id && w.UserId == userId);
    return wl is null ? Results.NotFound() : Results.Ok(wl);
}).RequireAuthorization();

app.MapPut("/work/{id}", (ClaimsPrincipal principal, string id, WorkLogUpdate req) =>
{
    var userId = principal.FindFirstValue(JwtRegisteredClaimNames.Sub)!;
    var wl = workLogs.FirstOrDefault(w => w.Id == id && w.UserId == userId);
    if (wl is null) return Results.NotFound();

    if (!string.IsNullOrWhiteSpace(req.Title)) wl.Title = req.Title.Trim();
    if (!string.IsNullOrWhiteSpace(req.Category)) wl.Category = req.Category.Trim();
    if (req.Minutes.HasValue && req.Minutes.Value > 0) wl.Minutes = req.Minutes.Value;
    if (req.Date.HasValue) wl.Date = req.Date.Value.Date;
    wl.Notes = req.Notes?.Trim();

    Save(workFile, workLogs);
    return Results.Ok(wl);
}).RequireAuthorization();

app.MapDelete("/work/{id}", (ClaimsPrincipal principal, string id) =>
{
    var userId = principal.FindFirstValue(JwtRegisteredClaimNames.Sub)!;
    var wl = workLogs.FirstOrDefault(w => w.Id == id && w.UserId == userId);
    if (wl is null) return Results.NotFound();
    workLogs.Remove(wl);
    Save(workFile, workLogs);
    return Results.NoContent();
}).RequireAuthorization();

// ----------------------
// 7) Reports
// ----------------------
app.MapGet("/report/daily", (ClaimsPrincipal principal, DateTime? date) =>
{
    var userId = principal.FindFirstValue(JwtRegisteredClaimNames.Sub)!;
    var day = (date ?? DateTime.UtcNow).Date;
    var items = workLogs.Where(w => w.UserId == userId && w.Date.Date == day).ToList();

    var report = new DailyReport
    {
        Date = day.ToString("yyyy-MM-dd"),
        TotalMinutes = items.Sum(i => i.Minutes),
        ByCategory = items.GroupBy(i => i.Category).ToDictionary(g => g.Key, g => g.Sum(x => x.Minutes)),
        Entries = items.OrderBy(i => i.Category).ThenBy(i => i.Title).ToList()
    };

    return Results.Ok(report);
}).RequireAuthorization();

app.MapGet("/report/range", (ClaimsPrincipal principal, DateTime from, DateTime to) =>
{
    if (to < from) return Results.BadRequest(new { message = "`to` must be >= `from`." });

    var userId = principal.FindFirstValue(JwtRegisteredClaimNames.Sub)!;
    var items = workLogs.Where(w => w.UserId == userId && w.Date.Date >= from.Date && w.Date.Date <= to.Date).ToList();

    var perDay = items
        .GroupBy(i => i.Date.Date)
        .OrderBy(g => g.Key)
        .ToDictionary(
            g => g.Key.ToString("yyyy-MM-dd"),
            g => g.Sum(x => x.Minutes));

    var perCategory = items
        .GroupBy(i => i.Category)
        .OrderBy(g => g.Key)
        .ToDictionary(g => g.Key, g => g.Sum(x => x.Minutes));

    var total = items.Sum(i => i.Minutes);

    return Results.Ok(new
    {
        from = from.ToString("yyyy-MM-dd"),
        to = to.ToString("yyyy-MM-dd"),
        totalMinutes = total,
        perDay,
        perCategory,
        count = items.Count
    });
}).RequireAuthorization();

// ----------------------
// 8) Root -> Swagger
// ----------------------
app.MapGet("/", () => Results.Redirect("/swagger"));

app.Run();
