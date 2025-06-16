using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WellnessTracker.Models;

namespace WellnessTracker.Controllers
{
    public class WorkoutLogEntriesController : Controller
    {
        private readonly AppDbContext _context;

        public WorkoutLogEntriesController(AppDbContext context)
        {
            _context = context;
        }

        private string GetUserId() =>
            User.FindFirstValue(ClaimTypes.NameIdentifier);

        private bool UserHasProfile(string userId) =>
            _context.UserProfile.Any(up => up.UserId == userId);

        public async Task<IActionResult> Index(int weekOffset = 0)
        {
            var userId = GetUserId();

            if (!UserHasProfile(userId))
                return RedirectToAction("Create", "UserProfile");

            var today = DateTime.Today;
            var weekStart = today.AddDays(-(int)(today.DayOfWeek == DayOfWeek.Sunday ? 6 : today.DayOfWeek - DayOfWeek.Monday))
                                 .AddDays(weekOffset * 7);
            var weekEnd = weekStart.AddDays(6);

            var userGoal = await _context.UserGoals
                .Where(g => g.UserId == userId)
                .Select(g => g.WorkoutGoal)
                .FirstOrDefaultAsync();

            var entries = await _context.WorkoutLogEntries
                .Where(e => e.UserId == userId && e.Date.Date >= weekStart && e.Date.Date <= weekEnd)
                .OrderByDescending(e => e.Date)
                .ToListAsync();

            var todayWorkout = entries
                .Where(e => e.Date.Date == today)
                .Sum(e => e.Duration);

            var totalCaloriesToday = entries
                .Where(e => e.Date.Date == today)
                .Sum(e => e.CaloriesBurned);

            var weeklyWorkout = new Dictionary<string, double>();
            for (int i = 0; i < 7; i++)
            {
                var date = weekStart.AddDays(i).Date;
                var label = date.ToString("ddd");
                weeklyWorkout[label] = entries.Where(e => e.Date.Date == date).Sum(e => e.Duration);
            }

            var weeklyAverage = weeklyWorkout.Values.Count > 0 ? weeklyWorkout.Values.Average() : 0;
            var workoutPercentage = (userGoal > 0) ? Math.Min(100, (int)((todayWorkout / userGoal) * 100)) : 0;

            ViewBag.WorkoutGoal = userGoal;
            ViewBag.TodayWorkout = todayWorkout;
            ViewBag.TotalCaloriesToday = totalCaloriesToday;
            ViewBag.WeeklyWorkout = weeklyWorkout;
            ViewBag.WeeklyAverageWorkout = weeklyAverage;
            ViewBag.WeekOffset = weekOffset;
            ViewBag.WorkoutPercentage = workoutPercentage;

            return View(entries);
        }

        public IActionResult WorkoutSummary(DateTime? date)
        {
            var selectedDate = date ?? DateTime.Today;
            var userId = GetUserId();

            var logs = _context.WorkoutLogEntries
                .Where(w => w.Date.Date == selectedDate.Date && w.UserId == userId)
                .ToList();

            ViewBag.SelectedDate = selectedDate;
            ViewBag.TotalWorkoutHours = logs.Sum(w => w.Duration);
            ViewBag.TotalCaloriesBurned = logs.Sum(w => w.CaloriesBurned);

            return View();
        }

        [HttpGet]
        public async Task<IActionResult> SetGoal()
        {
            var userId = GetUserId();
            var userGoal = await _context.UserGoals.FirstOrDefaultAsync(g => g.UserId == userId);
            var profile = await _context.UserProfile.FirstOrDefaultAsync(p => p.UserId == userId);

            int? recommendedWorkout = profile != null ? 30 : null;

            ViewBag.RecommendedWorkout = recommendedWorkout;
            return View(userGoal ?? new UserGoals());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetGoal(UserGoals userGoal)
        {
            var userId = GetUserId();
            userGoal.UserId = userId;

            var existingGoal = await _context.UserGoals.FirstOrDefaultAsync(g => g.UserId == userId);
            if (existingGoal != null)
            {
                existingGoal.WorkoutGoal = userGoal.WorkoutGoal;
                _context.Update(existingGoal);
            }
            else
            {
                _context.Add(userGoal);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(WorkoutLogEntry entry)
        {
            entry.UserId = GetUserId();
            if (ModelState.IsValid)
            {
                _context.Add(entry);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(entry);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var entry = await _context.WorkoutLogEntries.FirstOrDefaultAsync(m => m.Id == id);
            if (entry == null || entry.UserId != GetUserId()) return Unauthorized();

            return View(entry);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var entry = await _context.WorkoutLogEntries.FindAsync(id);
            if (entry == null || entry.UserId != GetUserId()) return Unauthorized();

            return View(entry);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Date,ExerciseName,Duration,CaloriesBurned,Notes")] WorkoutLogEntry entry)
        {
            if (id != entry.Id) return NotFound();

            var original = await _context.WorkoutLogEntries.AsNoTracking().FirstOrDefaultAsync(e => e.Id == id);
            if (original == null || original.UserId != GetUserId()) return Unauthorized();

            entry.UserId = original.UserId;

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(entry);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!WorkoutEntryExists(entry.Id)) return NotFound();
                    throw;
                }
            }

            return View(entry);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var entry = await _context.WorkoutLogEntries.FirstOrDefaultAsync(m => m.Id == id);
            if (entry == null || entry.UserId != GetUserId()) return Unauthorized();

            return View(entry);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var entry = await _context.WorkoutLogEntries.FindAsync(id);
            if (entry == null || entry.UserId != GetUserId()) return Unauthorized();

            _context.WorkoutLogEntries.Remove(entry);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool WorkoutEntryExists(int id) =>
            _context.WorkoutLogEntries.Any(e => e.Id == id);
    }
}
