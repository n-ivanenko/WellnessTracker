using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Security.Claims;
using System;
using System.Threading.Tasks;
using WellnessTracker.Models;

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

            if (!User.Identity.IsAuthenticated || userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var selectedDate = date ?? DateTime.Today;

            // Get user profile
            var profile = await _context.UserProfile.FirstOrDefaultAsync(p => p.UserId == userId);
            ViewBag.UserProfile = profile;
            ViewBag.SelectedDate = selectedDate.ToString("yyyy-MM-dd");

            // Calculate recommended goals
            if (profile != null)
            {
                double weightKg = profile.WeightLb * 0.453592;
                double heightCm = profile.HeightIn * 2.54;
                double bmr = 10 * weightKg + 6.25 * heightCm - 5 * profile.Age + 5;
                int recommendedCalories = (int)(bmr * 1.5);
                int recommendedSleep = profile.Age < 18 ? 9 : 8;
                int recommendedWorkout = 30;

                ViewBag.RecommendedCalories = recommendedCalories;
                ViewBag.RecommendedSleep = recommendedSleep;
                ViewBag.RecommendedWorkout = recommendedWorkout;
            }
            else
            {
                ViewBag.RecommendedCalories = null;
                ViewBag.RecommendedSleep = null;
                ViewBag.RecommendedWorkout = null;
            }

            // Get daily stats
            var totalCalories = await _context.CalorieLogEntries
                .Where(c => c.UserId == userId && c.Date.Date == selectedDate)
                .SumAsync(c => (double?)c.Calories) ?? 0;

            var totalSleep = await _context.SleepLogEntries
                .Where(s => s.UserId == userId && s.Date.Date == selectedDate)
                .SumAsync(s => (double?)s.HoursSlept) ?? 0;

            var workouts = await _context.WorkoutLogEntries
                .Where(w => w.UserId == userId && w.Date.Date == selectedDate)
                .ToListAsync();

            double totalWorkoutHours = workouts.Sum(w => w.Duration); // Assuming Duration is double (in minutes)
            double totalWorkoutCalories = workouts.Sum(w => w.CaloriesBurned);

            ViewBag.TotalCalories = totalCalories;
            ViewBag.TotalSleep = totalSleep;
            ViewBag.TotalWorkoutHours = totalWorkoutHours;
            ViewBag.TotalWorkoutCalories = totalWorkoutCalories;

            // Get user goals
            var userGoals = await _context.UserGoals.FirstOrDefaultAsync(g => g.UserId == userId);

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

            // Habit tracking progress
            var allHabits = await _context.HabitEntries
                .Where(h => h.UserId == userId)
                .ToListAsync();
            int totalHabits = allHabits.Count;

            var todayCompletions = await _context.HabitCompletions
                .Where(c => c.UserId == userId && c.Date.Date == selectedDate)
                .Select(c => c.HabitEntryId)  // Assuming HabitCompletions links to HabitEntryId
                .Distinct()
                .ToListAsync();
            int completedHabits = todayCompletions.Count;

            ViewBag.TotalHabits = totalHabits;
            ViewBag.HabitsCompleted = completedHabits;

            // Get mood for selected day
            var moodEntry = await _context.MoodEntries
                .Where(m => m.UserId == userId && m.Date.Date == selectedDate)
                .FirstOrDefaultAsync();

            if (moodEntry != null)
            {
                ViewBag.MoodRating = moodEntry.MoodRating; 
            }
            else
            {
                ViewBag.MoodRating = null;
                ViewBag.MoodNote = null;
            }

            return View();
        }
    }
}
