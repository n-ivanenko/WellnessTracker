using System;
using System.ComponentModel.DataAnnotations;

namespace WellnessTracker.Models
{
    public class CalorieLogEntry
    {
        public int Id { get; set; }

        [Required]
        public DateTime Date { get; set; }

        [Required]
        public string FoodItem { get; set; }

        [Required]
        public double Calories { get; set; }

        [MaxLength(1000)]
        public string? Notes { get; set; } 
    }
}

