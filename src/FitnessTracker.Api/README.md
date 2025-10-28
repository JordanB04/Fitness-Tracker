# 🏋️‍♀️ Fitness Tracker API  
**ASP.NET Core 8.0 | C# | Docker | JWT Authentication**

The **Fitness Tracker API** helps users track daily calorie intake, view meal history, and receive exercise recommendations.  
Built with **ASP.NET Core Web API**, it demonstrates secure authentication, clean architecture, and Dockerized deployment for consistent performance.

---

### 🚀 Features
- 🔐 JWT-based user authentication  
- 🍎 Log and retrieve calorie entries  
- 🏋️ Exercise recommendations  
- 💓 Health check endpoint (`/health`)  
- 🧪 xUnit testing + Swagger documentation  
- 🐳 Docker support for easy setup  

---

### ⚙️ Run Locally
```bash
dotnet restore
dotnet build
dotnet run --project src/FitnessTracker.Api/FitnessTracker.Api.csproj
http://localhost:8080/swagger
