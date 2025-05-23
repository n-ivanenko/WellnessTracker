namespace WellnessTracker.Models
{
    public class HabitCompletion
    {
        public int Id { get; set; }
        public int HabitEntryId { get; set; }
        public HabitEntry HabitEntry { get; set; }
        public DateTime Date { get; set; }
        public bool IsCompleted { get; set; }
        public string? UserId { get; set; }
    }

}
