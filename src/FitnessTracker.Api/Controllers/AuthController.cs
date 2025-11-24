using System;
using System.Linq;
using System.Security.Claims;
using System.Text;
using FitnessTracker.Api.Data;
using FitnessTracker.Api.Dtos;
using FitnessTracker.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;

namespace FitnessTracker.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _config;

        public AuthController(IConfiguration config)
        {
            _config = config;
        }

        [HttpPost("register")]
        public IActionResult Register(RegisterRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.Username) || string.IsNullOrWhiteSpace(req.Password))
            {
                return BadRequest(new { message = "Username and password are required" });
            }

            if (FakeDatabase.Users.Any(u => u.Username.Equals(req.Username, StringComparison.OrdinalIgnoreCase)))
            {
                return Conflict(new { message = "Username already taken" });
            }

            var newUser = new User
            {
                Id = FakeDatabase.Users.Count + 1,
                Username = req.Username,
                Password = req.Password   // NOTE: plain text for this class project
            };

            FakeDatabase.Users.Add(newUser);

            return Ok(new { message = "Registered", newUser.Id });
        }

        [HttpPost("login")]
        public IActionResult Login(LoginRequest req)
        {
            var user = FakeDatabase.Users
                .FirstOrDefault(u =>
                    u.Username.Equals(req.Username, StringComparison.OrdinalIgnoreCase) &&
                    u.Password == req.Password);

            if (user == null)
            {
                return Unauthorized(new { message = "Invalid credentials" });
            }

            var token = GenerateToken(user);
            return Ok(new AuthResponse { Token = token, Username = user.Username });
        }

        private string GenerateToken(User user)
        {
            var keyStr = _config["Jwt:Key"] ?? "SUPER_SECRET_KEY_1234567890_CHANGE_ME";
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyStr));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Name, user.Username)
                }),
                Expires = DateTime.UtcNow.AddHours(4),
                SigningCredentials = creds
            };

            var handler = new JwtSecurityTokenHandler();
            return handler.WriteToken(handler.CreateToken(tokenDescriptor));
        }
    }
}
