using System;
using System.ComponentModel.DataAnnotations;

namespace WellnessTracker.Models
{
    public class SleepLogEntry
    {
        public int Id { get; set; }

        [Required]
        public DateTime Date { get; set; }

        [Required]
        public double HoursSlept { get; set; } 

        [Required]
        public string SleepQuality { get; set; } 

        [MaxLength(1000)]
        public string? Notes { get; set; } 
    }
}
