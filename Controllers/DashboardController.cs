using Microsoft.AspNetCore.Mvc;
using WellnessTracker.Models;
using WellnessTracker.ViewModels;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Security.Claims;
using System;
using System.Threading.Tasks;

namespace WellnessTracker.Controllers
{
    public class DashboardController : Controller
    {
        private readonly AppDbContext _context;

        public DashboardController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
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

            var totalWorkoutDuration = await _context.WorkoutEntries
                .Where(w => w.UserId == userId && w.Date.Date == today)
                .SumAsync(w => (int?)w.Duration) ?? 0;

            var totalHabitsCompleted = await _context.HabitCompletions
                .Where(h => h.UserId == userId && h.Date.Date == today)
                .CountAsync();

            var totalHabits = await _context.HabitEntries
                .Where(h => h.UserId == userId)
                .CountAsync();

            var viewModel = new DashboardViewModel
            {
                MoodToday = mood ?? "Not logged",
                SleepToday = sleep ?? "Not logged",
                CalorieToday = $"{totalCalories} kcal",
                WorkoutToday = $"{totalWorkoutDuration} mins",
                HabitToday = $"{totalHabitsCompleted} of {totalHabits} completed"
            };

            return View(viewModel);
        }

    }
}

