using System;
using System.ComponentModel.DataAnnotations;

namespace WellnessTracker.Models
{
    public class HabitEntry
    {
        public int Id { get; set; }
        [Required]
        public string HabitName { get; set; } 
        [Required]
        public DateTime StartDate { get; set; } 
        [Required]
        public DateTime TargetDate { get; set; } 
        public bool IsCompleted { get; set; } 
        [MaxLength(1000)]
        public string? Notes { get; set; }
        public string? UserId { get; set; }
    }
}

