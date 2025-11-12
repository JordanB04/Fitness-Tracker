<<<<<<< HEAD
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
=======
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BCrypt.Net;
using Microsoft.AspNetCore.Mvc;
>>>>>>> origin/main
using Microsoft.IdentityModel.Tokens;
using FitnessTracker.Api.Models;

namespace FitnessTracker.Api.Controllers
{
    [ApiController]
<<<<<<< HEAD
    [Route("auth")]
    public class AuthController : ControllerBase
    {
        private static readonly List<User> Users = new();

        private readonly string jwtKey = "SUPER_SECRET_KEY_CHANGE_THIS_64_CHAR_STRING_1234567890_ABCDEFGHIJKLMN";

        [HttpPost("register")]
        public IActionResult Register([FromBody] RegisterRequest request)
        {
            if (Users.Any(u => u.Username == request.Username))
                return BadRequest("Username already exists");

            Users.Add(new User { Username = request.Username, Password = request.Password });
            return Ok("User registered successfully");
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            var user = Users.FirstOrDefault(u => u.Username == request.Username && u.Password == request.Password);
            if (user == null) return Unauthorized("Invalid credentials");

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(jwtKey);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, request.Username) }),
                Expires = DateTime.UtcNow.AddHours(2),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return Ok(new { token = tokenHandler.WriteToken(token) });
=======
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly string _userDataPath = Path.Combine("data", "users.json");
        private readonly string _jwtSecret;

        public AuthController(IConfiguration config)
        {
            _jwtSecret = config["JwtKey"] ?? Environment.GetEnvironmentVariable("JWT_SECRET") 
                ?? "SUPER_SECRET_KEY_CHANGE_THIS_64_CHAR_STRING_1234567890_ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        }

        [HttpPost("register")]
        public IActionResult Register([FromBody] User user)
        {
            if (string.IsNullOrWhiteSpace(user.Username) || string.IsNullOrWhiteSpace(user.Password))
                return BadRequest("Username and password are required.");

            var users = LoadUsers();
            if (users.Any(u => u.Username == user.Username))
                return Conflict("Username already exists.");

            user.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);
            users.Add(user);
            SaveUsers(users);

            return Ok(new { message = "User registered successfully." });
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] User login)
        {
            var users = LoadUsers();
            var user = users.FirstOrDefault(u => u.Username == login.Username);

            if (user == null || !BCrypt.Net.BCrypt.Verify(login.Password, user.Password))
                return Unauthorized("Invalid username or password.");

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_jwtSecret);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.Name, user.Username)
                }),
                Expires = DateTime.UtcNow.AddHours(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var jwt = tokenHandler.WriteToken(token);

            return Ok(new { token = jwt });
        }

        private List<User> LoadUsers()
        {
            if (!System.IO.File.Exists(_userDataPath))
                return new List<User>();

            var json = System.IO.File.ReadAllText(_userDataPath);
            return string.IsNullOrWhiteSpace(json)
                ? new List<User>()
                : System.Text.Json.JsonSerializer.Deserialize<List<User>>(json) ?? new List<User>();
        }

        private void SaveUsers(List<User> users)
        {
            var json = System.Text.Json.JsonSerializer.Serialize(users, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
            System.IO.File.WriteAllText(_userDataPath, json);
>>>>>>> origin/main
        }
    }
}
