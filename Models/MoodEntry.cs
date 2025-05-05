using System;
using System.ComponentModel.DataAnnotations;

namespace WellnessTracker.Models
{
    public class MoodEntry
    {
        public int Id { get; set; }

        [Required]
        public DateTime Date { get; set; }

        [Required]
        [Range(1, 10)]
        public int MoodRating { get; set; } 

        [MaxLength(1000)]
        public string? Notes { get; set; } 
    }
}
