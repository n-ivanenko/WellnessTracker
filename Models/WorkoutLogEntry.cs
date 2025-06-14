using System;
using System.ComponentModel.DataAnnotations;

namespace WellnessTracker.Models
{
    public class WorkoutLogEntry
    {
        public int Id { get; set; }
        [Required]

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime Date { get; set; }
        [Required]
        public string ExerciseName { get; set; }
        [Required]
        public double Duration { get; set; }
        [Range(1, 10000)]
        public double CaloriesBurned { get; set; }
        [MaxLength(1000)]
        public string? Notes { get; set; }
        public string? UserId { get; set; }
    }
}
