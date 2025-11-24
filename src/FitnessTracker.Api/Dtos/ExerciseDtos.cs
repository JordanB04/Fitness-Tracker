using System;

namespace FitnessTracker.Api.Dtos
{
    public class ExerciseLogRequest
    {
        public string ExerciseName { get; set; } = string.Empty;
        public int CaloriesBurned { get; set; }
        public DateTime Date { get; set; }
    }

    // For future extension – optional
    public class SuggestionRequest
    {
        public double CurrentWeightKg { get; set; }
        public double TargetWeightKg { get; set; }
        public string PreferredExerciseType { get; set; } = "walking";
    }

    public class ExerciseSuggestion
    {
        public string ExerciseType { get; set; } = string.Empty;
        public int DurationMinutes { get; set; }
        public double EstimatedCaloriesBurned { get; set; }
        public string Note { get; set; } = string.Empty;
    }
}
