using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using FitnessTracker.Api.Models;

var builder = WebApplication.CreateBuilder(args);

// ------------------------------------------------
// 1️⃣  JWT Key Configuration
// ------------------------------------------------
var jwtKey = Environment.GetEnvironmentVariable("JWT_SECRET") ??
             "SUPER_SECRET_KEY_CHANGE_THIS_64_CHAR_STRING_1234567890_ABCDEFGHIJKLMNOPQRSTUVWXYZ";

var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));

// ------------------------------------------------
// 2️⃣  Add Services to the container
// ------------------------------------------------
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
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// ------------------------------------------------
// 3️⃣  Swagger Configuration with JWT Support
// ------------------------------------------------
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Fitness Tracker API",
        Version = "v1",
        Description = "API for tracking fitness, calories, and user activity"
    });

    // Enable JWT Auth in Swagger UI
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: 'Bearer {token}'",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer"
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
            new string[] {}
        }
    });
});

// ------------------------------------------------
// 4️⃣  Build the App
// ------------------------------------------------
var app = builder.Build();

// ------------------------------------------------
// 5️⃣  Configure Middleware Pipeline
// ------------------------------------------------
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

// ------------------------------------------------
// 6️⃣  Map Controllers and Health Endpoint
// ------------------------------------------------
app.MapControllers();

app.MapGet("/health", () => new { status = "ok", environment = app.Environment.EnvironmentName });

// ------------------------------------------------
// 7️⃣  Run the Application
// ------------------------------------------------
app.Run();
