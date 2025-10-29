🚀🏋️‍♀️ Fitness Tracker API (v1.0)

Stable version: v1.0
A lightweight .NET 8 Minimal API that tracks calories, workouts, and daily summaries — designed for simplicity, persistence, and easy integration with Swagger UI.

✨ Features

🔐 User Authentication with JWT tokens

🍎 Calorie Tracking API (add, update, delete, and summarize)

🏋️‍♀️ Workout Logging with duration and exercise type

📊 Swagger UI Integration for live API testing

🩺 Health Check Endpoint → /health

💾 Persistent JSON Storage for local development

⚡ Automatic Swagger launch on startup → http://localhost:8080

🧱 Project Structure
Fitness-Tracker/
├── src/
│   └── FitnessTracker.Api/
│       ├── Controllers/
│       │   ├── AuthController.cs
│       │   └── CalorieController.cs
│       ├── Models/
│       │   ├── AuthModels.cs
│       │   ├── WorkModels.cs
│       │   └── ReportModels.cs
│       ├── Program.cs
│       └── appsettings.json
├── data/
│   ├── users.json
│   └── calories.json
├── .gitignore
├── FitnessTracker.sln
└── README.md
# 1️⃣ Build the solution
dotnet build

# 2️⃣ Run the API
dotnet run --project src/FitnessTracker.Api

# 3️⃣ Open Swagger UI
http://localhost:8080/swagger
