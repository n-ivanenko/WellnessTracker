using System;
using System.ComponentModel.DataAnnotations;

namespace WellnessTracker.Models
{
    public class MoodEntry
    {
        public int Id { get; set; }
        [Required]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime Date { get; set; }
        [Required]
        [Range(1, 10)]
        public int MoodRating { get; set; } 
        [MaxLength(1000)]
        public string? Notes { get; set; }
        public string? UserId { get; set; }
    }
}
