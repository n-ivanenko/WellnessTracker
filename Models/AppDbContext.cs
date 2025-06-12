using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace WellnessTracker.Models
{
    public class AppDbContext : IdentityDbContext

    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        public DbSet<MoodEntry> MoodEntries { get; set; }
        public DbSet<WorkoutLogEntry> WorkoutLogEntries { get; set; }
        public DbSet<CalorieLogEntry> CalorieLogEntries { get; set; }
        public DbSet<SleepLogEntry> SleepLogEntries { get; set; }
        public DbSet<HabitEntry> HabitEntries { get; set; }
        public DbSet<HabitCompletion> HabitCompletions { get; set; }
        public DbSet<UserGoals> UserGoals { get; set; }
        public DbSet<UserProfile> UserProfile { get; set; }


    }
}
