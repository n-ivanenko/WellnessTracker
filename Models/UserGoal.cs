namespace WellnessTracker.Models
{
    public class UserGoal
    {
        public int Id { get; set; }
        public string UserId { get; set; }  // Link to Identity user
        public double? CalorieGoal { get; set; }
        public double? SleepGoal { get; set; }
        public int? WorkoutGoal { get; set; }
    }
}
