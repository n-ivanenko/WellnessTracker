using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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

        public IActionResult WorkoutSummary(DateTime? date)
        {
            var selectedDate = date ?? DateTime.Today;

            string? userId = null;
            if (User.Identity.IsAuthenticated)
            {
                userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            }

            var logs = _context.WorkoutLogEntries
                .Where(w => w.Date.Date == selectedDate.Date && (userId == null || w.UserId == userId))
                .ToList();

            double totalDuration = logs.Sum(w => w.Duration);
            double totalCaloriesBurned = logs.Sum(w => w.CaloriesBurned);

            ViewBag.SelectedDate = selectedDate;
            ViewBag.TotalWorkoutHours = totalDuration;
            ViewBag.TotalCaloriesBurned = totalCaloriesBurned;

            return View();
        }
        // GET: WorkoutLogEntries/SetGoal
        [HttpGet]
        public async Task<IActionResult> SetGoal()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userGoal = await _context.UserGoals.FirstOrDefaultAsync(g => g.UserId == userId);

            return View(userGoal ?? new UserGoal());
        }

        // POST: WorkoutLogEntries/SetGoal
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetGoal(UserGoal userGoal)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
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
            return RedirectToAction("Index", "WorkoutLogEntries");
        }

        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var today = DateTime.Today;
            var userGoal = await _context.UserGoals.FirstOrDefaultAsync(g => g.UserId == userId);
            var workoutGoal = userGoal?.WorkoutGoal ?? 0;

            if (userGoal != null)
            {
                ViewBag.WorkoutGoal = userGoal.WorkoutGoal;
            }
            else
            {
                ViewBag.WorkoutGoal = null;
            }

            var entries = await _context.WorkoutLogEntries
                .Where(c => c.UserId == userId && c.Date.Date == today)
                .OrderByDescending(c => c.Date)
                .ToListAsync();

            var totalWorkoutToday = await _context.WorkoutLogEntries
                .Where(s => s.UserId == userId && s.Date.Date == today)
                .SumAsync(s => (double?)s.Duration) ?? 0;

            var weekStart = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek + (DateTime.Today.DayOfWeek == DayOfWeek.Sunday ? -6 : 1)); // Monday start
            var weekEnd = weekStart.AddDays(6);

            var weekEntries = entries
                .Where(e => e.Date.Date >= weekStart && e.Date.Date <= weekEnd)
                .GroupBy(e => e.Date.Date)
                .ToDictionary(g => g.Key, g => g.Sum(e => e.Duration));

            var weeklyAverage = weekEntries.Values.Count > 0 ? weekEntries.Values.Average() : 0;

            var todayWorkout = await _context.WorkoutLogEntries
                .Where(c => c.UserId == userId && c.Date.Date == today)
                .SumAsync(c => (double?)c.Duration) ?? 0;

            var weeklyWorkout = new Dictionary<string, double>();
            for (int i = 0; i < 7; i++)
            {
                var date = weekStart.AddDays(i).Date;
                var label = date.ToString("ddd");
                weeklyWorkout[label] = weekEntries.ContainsKey(date) ? weekEntries[date] : 0;
            }

            ViewBag.WorkoutGoal = userGoal.WorkoutGoal;
            ViewBag.WeeklyWorkout = weeklyWorkout;
            ViewBag.WeeklyAverageWorkout = weeklyAverage;
            ViewBag.TodayWorkout = todayWorkout;
            ViewBag.WorkoutPercentage = (userGoal.WorkoutGoal > 0) ? Math.Min(100, (int)((todayWorkout / userGoal.WorkoutGoal) * 100)) : 0;

            return View(entries);
        }

        // GET: WorkoutEntries/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var workoutEntry = await _context.WorkoutLogEntries
                .FirstOrDefaultAsync(m => m.Id == id);
            if (workoutEntry == null)
            {
                return NotFound();
            }

            return View(workoutEntry);
        }

        // GET: WorkoutEntries/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: WorkoutEntries/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Date,ExerciseName,Duration,CaloriesBurned,Notes")] WorkoutLogEntry workoutEntry)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            workoutEntry.UserId = userId;

            ModelState.Remove("UserId");

            if (ModelState.IsValid)
            {
                _context.Add(workoutEntry);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(workoutEntry);
        }

        // GET: WorkoutEntries/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var workoutEntry = await _context.WorkoutLogEntries.FindAsync(id);
            if (workoutEntry == null)
            {
                return NotFound();
            }
            return View(workoutEntry);
        }

        // POST: WorkoutEntries/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Date,ExerciseName,Duration,CaloriesBurned,Notes")] WorkoutLogEntry workoutEntry)
        {
            if (id != workoutEntry.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(workoutEntry);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!WorkoutEntryExists(workoutEntry.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(workoutEntry);
        }

        // GET: WorkoutEntries/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var workoutEntry = await _context.WorkoutLogEntries
                .FirstOrDefaultAsync(m => m.Id == id);
            if (workoutEntry == null)
            {
                return NotFound();
            }

            return View(workoutEntry);
        }

        // POST: WorkoutEntries/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var workoutEntry = await _context.WorkoutLogEntries.FindAsync(id);
            if (workoutEntry != null)
            {
                _context.WorkoutLogEntries.Remove(workoutEntry);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool WorkoutEntryExists(int id)
        {
            return _context.WorkoutLogEntries.Any(e => e.Id == id);
        }
    }
}
