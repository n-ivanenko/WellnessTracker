using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace WellnessTracker.Models
{
    public class AdminUserDetailsViewModel
    {
        public IdentityUser User { get; set; }
        public UserProfile Profile { get; set; }
        public List<MoodEntry> MoodEntries { get; set; }
        public List<CalorieLogEntry> CalorieLogs { get; set; }
        public List<SleepLogEntry> SleepLogs { get; set; }
        public List<WorkoutLogEntry> WorkoutLogs { get; set; }
        public List<HabitCompletion> HabitCompletions { get; set; }
        public List<HabitEntry> HabitEntries { get; set; }
    }
    public class EditUserViewModel
    {
        public string Id { get; set; }
        public string Email { get; set; }
        public string UserName { get; set; }
    }

    public class ResetPasswordViewModel
    {
        public string UserId { get; set; }
        public string NewPassword { get; set; }
    }
}
