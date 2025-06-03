using Microsoft.AspNetCore.Mvc;
using WellnessTracker.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Security.Claims;
using System;
using System.Threading.Tasks;

namespace WellnessTracker.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _context;

        public HomeController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(DateTime? date)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var today = DateTime.Today;

            var mood = await _context.MoodEntries
                .Where(m => m.UserId == userId && m.Date.Date == today)
                .Select(m => m.MoodRating.ToString())
                .FirstOrDefaultAsync();

            var sleep = await _context.SleepLogEntries
                .Where(s => s.UserId == userId && s.Date.Date == today)
                .Select(s => s.HoursSlept + " hrs")
                .FirstOrDefaultAsync();

            var totalCalories = await _context.CalorieLogEntries
                .Where(c => c.UserId == userId && c.Date.Date == today)
                .SumAsync(c => (double?)c.Calories) ?? 0;

            var totalWorkoutDuration = await _context.WorkoutLogEntries
                .Where(w => w.UserId == userId && w.Date.Date == today)
                .SumAsync(w => (int?)w.Duration) ?? 0;

            var totalHabitsCompleted = await _context.HabitCompletions
                .Where(h => h.UserId == userId && h.Date.Date == today)
                .CountAsync();

            var totalHabits = await _context.HabitEntries
                .Where(h => h.UserId == userId)
                .CountAsync();
            var selectedDate = date ?? DateTime.Today;

            string? UserId = null;
            if (User.Identity.IsAuthenticated)
            {
                userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            }

            //var totalCalories = _context.CalorieLogEntries
            //    .Where(c => c.Date.Date == selectedDate.Date && (userId == null || c.UserId == userId))
            //    .Sum(c => (double?)c.Calories) ?? 0;

            var totalSleep = _context.SleepLogEntries
                .Where(s => s.Date.Date == selectedDate.Date && (userId == null || s.UserId == userId))
                .Sum(s => (double?)s.HoursSlept) ?? 0;

            var workouts = _context.WorkoutLogEntries
                .Where(w => w.Date.Date == selectedDate.Date && (userId == null || w.UserId == userId))
                .ToList();

            var totalWorkoutHours = workouts.Sum(w => w.Duration);
            var totalWorkoutCalories = workouts.Sum(w => w.CaloriesBurned);

            ViewBag.SelectedDate = selectedDate.ToString("yyyy-MM-dd");
            ViewBag.TotalCalories = totalCalories;
            ViewBag.TotalSleep = totalSleep;
            ViewBag.TotalWorkoutHours = totalWorkoutHours;
            ViewBag.TotalWorkoutCalories = totalWorkoutCalories;

            return View();
        }

    }
}

