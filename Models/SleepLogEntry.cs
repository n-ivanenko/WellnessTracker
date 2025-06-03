using System;
using System.ComponentModel.DataAnnotations;

namespace WellnessTracker.Models
{
    public class SleepLogEntry
    {
        public int Id { get; set; }
        [Required]

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime Date { get; set; }
        [Required]
        public double HoursSlept { get; set; } 
        [Required]
        public string SleepQuality { get; set; } 
        [MaxLength(1000)]
        public string? Notes { get; set; }
        public string? UserId { get; set; }
    }
}
