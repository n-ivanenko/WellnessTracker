using System;
using System.ComponentModel.DataAnnotations;

namespace WellnessTracker.Models
{
    public class WorkoutEntry
    {
        public int Id { get; set; }

        [Required]
        public DateTime Date { get; set; }

        [Required]
        public string ExerciseName { get; set; }

        [Required]
        public double Duration { get; set; }

        [Range(1, 100)]
        public double CaloriesBurned { get; set; }

        [MaxLength(1000)]
        public string? Notes { get; set; }
    }
}
