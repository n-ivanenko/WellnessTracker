using Microsoft.EntityFrameworkCore;

namespace WellnessTracker.Models
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        public DbSet<MoodEntry> MoodEntries { get; set; }
        public DbSet<WorkoutEntry> WorkoutEntries { get; set; }
        public DbSet<CalorieLogEntry> CalorieLogEntries { get; set; }
        public DbSet<SleepLogEntry> SleepLogEntries { get; set; }
        public DbSet<HabitEntry> HabitEntries { get; set; }
    }
}
