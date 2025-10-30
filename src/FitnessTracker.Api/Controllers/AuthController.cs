using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using FitnessTracker.Models;
using System.Text.Json;

namespace FitnessTracker.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private static readonly string DataPath = Path.Combine("Data", "UserData.json");

        private List<User> LoadUsers()
        {
            if (!System.IO.File.Exists(DataPath))
                return new List<User>();

            var json = System.IO.File.ReadAllText(DataPath);
            return string.IsNullOrWhiteSpace(json) ? new List<User>() :
                JsonSerializer.Deserialize<List<User>>(json) ?? new List<User>();
        }

        private void SaveUsers(List<User> users)
        {
            var json = JsonSerializer.Serialize(users, new JsonSerializerOptions { WriteIndented = true });
            System.IO.File.WriteAllText(DataPath, json);
        }

        [HttpPost("register")]
        public IActionResult Register(User request)
        {
            var users = LoadUsers();
            if (users.Any(u => u.Username == request.Username))
                return BadRequest("User already exists.");

            users.Add(request);
            SaveUsers(users);
            return Ok("User registered successfully!");
        }

        [HttpPost("login")]
        public IActionResult Login(User request)
        {
            var users = LoadUsers();
            var user = users.FirstOrDefault(u =>
                u.Username == request.Username && u.Password == request.Password);

            if (user == null)
                return Unauthorized("Invalid credentials.");

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes("super_secret_key_1234567890");
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.Name, user.Username)
                }),
                Expires = DateTime.UtcNow.AddHours(1),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return Ok(new { token = tokenHandler.WriteToken(token) });
        }
    }
}
