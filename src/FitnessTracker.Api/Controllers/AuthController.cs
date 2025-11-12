using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace FitnessTracker.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        // Dummy in-memory user store for demonstration
        private static readonly Dictionary<string, string> _users = new()
        {
            { "testuser", "password123" }
        };

        private readonly IConfiguration _config;

        public AuthController(IConfiguration config)
        {
            _config = config;
        }

        /// <summary>
        /// Register a new user
        /// </summary>
        [HttpPost("register")]
        public IActionResult Register([FromBody] UserCredentials request)
        {
            if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
                return BadRequest("Username and password are required.");

            if (_users.ContainsKey(request.Username))
                return Conflict("User already exists.");

            _users.Add(request.Username, request.Password);
            return Ok("User registered successfully.");
        }

        /// <summary>
        /// Login and return a JWT token
        /// </summary>
        [HttpPost("login")]
        public IActionResult Login([FromBody] UserCredentials request)
        {
            if (!_users.TryGetValue(request.Username, out var storedPassword) || storedPassword != request.Password)
                return Unauthorized("Invalid username or password.");

            var token = GenerateJwtToken(request.Username);
            return Ok(new { token });
        }

        /// <summary>
        /// Generate a JWT token for authenticated users
        /// </summary>
        private string GenerateJwtToken(string username)
        {
            var key = Encoding.UTF8.GetBytes(_config["Jwt:Key"] ?? "super_secret_key_12345");
            var claims = new[]
            {
                new Claim(ClaimTypes.Name, username),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var credentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(2),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        // Simple DTO for login/register requests
        public class UserCredentials
        {
            public string Username { get; set; } = string.Empty;
            public string Password { get; set; } = string.Empty;
        }
    }
}
