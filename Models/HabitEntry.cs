using System;
using System.ComponentModel.DataAnnotations;

namespace WellnessTracker.Models
{
    public class HabitEntry
    {
        public int Id { get; set; }
        [Required]
        public string HabitName { get; set; } 
        public string? Notes { get; set; }
        public string? UserId { get; set; }
    }
}

