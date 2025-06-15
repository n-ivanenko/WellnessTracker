namespace WellnessTracker.Models
{
    public class UserGoals
    {
        public int Id { get; set; }
        public string UserId { get; set; } 
        public double? CalorieGoal { get; set; }
        public double? SleepGoal { get; set; }
        public double? WorkoutGoal { get; set; }
    }
}
