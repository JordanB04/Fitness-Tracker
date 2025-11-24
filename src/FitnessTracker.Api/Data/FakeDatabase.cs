using System.Collections.Generic;
using FitnessTracker.Api.Models;

namespace FitnessTracker.Api.Data
{
    public static class FakeDatabase
    {
        // === USERS ============================================================
        public static List<User> Users { get; set; } = new List<User>();

        // === CALORIE ENTRIES ==================================================
        public static List<CalorieEntry> Calories { get; set; } = new List<CalorieEntry>();
        private static int _calorieId = 1;
        public static int NextCalorieId() => _calorieId++;

        // === EXERCISE ENTRIES =================================================
        public static List<ExerciseEntry> Exercises { get; set; } = new List<ExerciseEntry>();
        private static int _exerciseId = 1;
        public static int NextExerciseId() => _exerciseId++;
    }
}
