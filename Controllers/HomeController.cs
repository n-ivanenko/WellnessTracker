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
            var selectedDate = date ?? DateTime.Today;

            if (!User.Identity.IsAuthenticated || userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var mood = await _context.MoodEntries
                .Where(m => m.UserId == userId && m.Date.Date == selectedDate)
                .Select(m => m.MoodRating.ToString())
                .FirstOrDefaultAsync();

            var sleep = await _context.SleepLogEntries
                .Where(s => s.UserId == userId && s.Date.Date == selectedDate)
                .Select(s => s.HoursSlept + " hrs")
                .FirstOrDefaultAsync();

            var totalCalories = await _context.CalorieLogEntries
                .Where(c => c.UserId == userId && c.Date.Date == selectedDate)
                .SumAsync(c => (double?)c.Calories) ?? 0;

            var totalWorkoutDuration = await _context.WorkoutLogEntries
                .Where(w => w.UserId == userId && w.Date.Date == selectedDate)
                .SumAsync(w => (int?)w.Duration) ?? 0;

            var totalHabitsCompleted = await _context.HabitCompletions
                .Where(h => h.UserId == userId && h.Date.Date == selectedDate)
                .CountAsync();

            var totalHabits = await _context.HabitEntries
                .Where(h => h.UserId == userId)
                .CountAsync();

            var totalSleep = await _context.SleepLogEntries
                .Where(s => s.Date.Date == selectedDate && s.UserId == userId)
                .SumAsync(s => (double?)s.HoursSlept) ?? 0;

            var workouts = await _context.WorkoutLogEntries
                .Where(w => w.Date.Date == selectedDate && w.UserId == userId)
                .ToListAsync();

            var totalWorkoutHours = workouts.Sum(w => w.Duration);
            var totalWorkoutCalories = workouts.Sum(w => w.CaloriesBurned);
            var userGoals = await _context.UserGoals.FirstOrDefaultAsync(g => g.UserId == userId);

            ViewBag.SelectedDate = selectedDate.ToString("yyyy-MM-dd");
            ViewBag.TotalCalories = totalCalories;
            ViewBag.TotalSleep = totalSleep;
            ViewBag.TotalWorkoutHours = totalWorkoutHours;
            ViewBag.TotalWorkoutCalories = totalWorkoutCalories;

            if (userGoals != null)
            {
                ViewBag.CalorieGoal = userGoals.CalorieGoal;
                ViewBag.SleepGoal = userGoals.SleepGoal;
                ViewBag.WorkoutGoal = userGoals.WorkoutGoal;

                ViewBag.CaloriesStatus = totalCalories >= (userGoals.CalorieGoal ?? 0) ? "On Track" : "Below Goal";
                ViewBag.SleepStatus = totalSleep >= (userGoals.SleepGoal ?? 0) ? "On Track" : "Below Goal";
                ViewBag.WorkoutStatus = totalWorkoutHours >= (userGoals.WorkoutGoal ?? 0) ? "On Track" : "Below Goal";
            }
            else
            {
                ViewBag.CalorieGoal = null;
                ViewBag.SleepGoal = null;
                ViewBag.WorkoutGoal = null;
                ViewBag.CaloriesStatus = ViewBag.SleepStatus = ViewBag.WorkoutStatus = "No Goal Set";
            }

            return View();

        }
    }
}
