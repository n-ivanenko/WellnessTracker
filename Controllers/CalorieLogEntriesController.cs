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
    public class CalorieLogEntriesController : Controller
    {
        private readonly AppDbContext _context;

        public CalorieLogEntriesController(AppDbContext context)
        {
            _context = context;
        }
        private bool UserHasProfile(string userId)
        {
            return _context.UserProfile.Any(up => up.UserId == userId);
        }

        public IActionResult CalorieSummary(DateTime? date)
        {
            var selectedDate = date ?? DateTime.Today;

            var userId = User.Identity.IsAuthenticated ? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier).Value : null;

            var totalCalories = _context.CalorieLogEntries
                .Where(c => c.Date.Date == selectedDate.Date && (userId == null || c.UserId == userId))
                .Sum(c => (double?)c.Calories) ?? 0;

            ViewBag.SelectedDate = selectedDate;
            ViewBag.TotalCalories = totalCalories;

            return View();
        }
        // GET: CalorieLogEntries/SetGoal
        [HttpGet]
        public async Task<IActionResult> SetGoal()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userGoal = await _context.UserGoals.FirstOrDefaultAsync(g => g.UserId == userId);
            var profile = await _context.UserProfile.FirstOrDefaultAsync(p => p.UserId == userId);

            double? recommendedCalories = null;

            if (profile != null)
            {
                double weightKg = profile.WeightLb * 0.453592;
                double heightCm = profile.HeightIn * 2.54;
                double bmr = 10 * weightKg + 6.25 * heightCm - 5 * profile.Age + 5;
                recommendedCalories = Math.Round(bmr * 1.5);
            }

            ViewBag.RecommendedCalories = recommendedCalories;
            return View(userGoal ?? new UserGoals());
        }

        // POST: CalorieLogEntries/SetGoal
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetGoal(UserGoals goal)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var existingGoal = await _context.UserGoals.FirstOrDefaultAsync(g => g.UserId == userId);

            ModelState.Remove("UserId");

            if (ModelState.IsValid)
            {
                if (existingGoal != null)
                {
                    existingGoal.CalorieGoal = goal.CalorieGoal;
                    _context.Update(existingGoal);
                }
                else
                {
                    goal.UserId = userId;
                    _context.UserGoals.Add(goal);
                }

                await _context.SaveChangesAsync();
                return RedirectToAction("Index", "CalorieLogEntries");
            }

            var profile = await _context.UserProfile.FirstOrDefaultAsync(p => p.UserId == userId);
            if (profile != null)
            {
                double weightKg = profile.WeightLb * 0.453592;
                double heightCm = profile.HeightIn * 2.54;
                double bmr = 10 * weightKg + 6.25 * heightCm - 5 * profile.Age + 5;
                ViewBag.RecommendedCalories = Math.Round(bmr * 1.5);
            }

            return View(goal);
        }

        public async Task<IActionResult> Index(int weekOffset = 0)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!UserHasProfile(userId))
            {
                return RedirectToAction("Create", "UserProfile");
            }

            var today = DateTime.Today;

            var baseDate = today.AddDays(weekOffset * 7);
            var weekStart = baseDate.AddDays(-(int)baseDate.DayOfWeek + (baseDate.DayOfWeek == DayOfWeek.Sunday ? -6 : 1)); // Monday
            var weekEnd = weekStart.AddDays(6);

            var entries = await _context.CalorieLogEntries
                .Where(e => e.UserId == userId)
                .OrderByDescending(e => e.Date)
                .ToListAsync();

            var filteredEntries = entries
                .Where(e => e.Date.Date >= weekStart && e.Date.Date <= weekEnd)
                .ToList();

            var userGoal = await _context.UserGoals
                .Where(g => g.UserId == userId)
                .Select(g => g.CalorieGoal)
                .FirstOrDefaultAsync();

            var weekEntries = filteredEntries
                .GroupBy(e => e.Date.Date)
                .ToDictionary(g => g.Key, g => g.Sum(e => e.Calories));

            var weeklyAverage = weekEntries.Values.Count > 0 ? weekEntries.Values.Average() : 0;

            var todayCalories = await _context.CalorieLogEntries
                .Where(c => c.UserId == userId && c.Date.Date == today)
                .SumAsync(c => (double?)c.Calories) ?? 0;

            var weeklyCalories = new Dictionary<string, double>();
            for (int i = 0; i < 7; i++)
            {
                var date = weekStart.AddDays(i).Date;
                var label = date.ToString("ddd");
                weeklyCalories[label] = weekEntries.ContainsKey(date) ? weekEntries[date] : 0;
            }

            ViewBag.WeeklyCalories = weeklyCalories;
            ViewBag.WeeklyAverageCalories = weeklyAverage;
            ViewBag.CalorieGoal = userGoal;
            ViewBag.TodayCalories = todayCalories;
            ViewBag.CaloriesPercentage = (userGoal > 0) ? Math.Min(100, (int)((todayCalories / userGoal) * 100)) : 0;

            ViewBag.WeekOffset = weekOffset;

            return View(filteredEntries);
        }

        // GET: CalorieLogEntries/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var calorieLogEntry = await _context.CalorieLogEntries
                .FirstOrDefaultAsync(m => m.Id == id);
            if (calorieLogEntry == null)
            {
                return NotFound();
            }

            return View(calorieLogEntry);
        }

        // GET: CalorieLogEntries/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: CalorieLogEntries/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CalorieLogEntry entry)
        {
            entry.UserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (ModelState.IsValid)
            {
                _context.Add(entry);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(entry);
        }


        // GET: CalorieLogEntries/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var calorieLogEntry = await _context.CalorieLogEntries.FindAsync(id);
            if (calorieLogEntry == null)
            {
                return NotFound();
            }
            return View(calorieLogEntry);
        }

        // POST: CalorieLogEntries/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Date,FoodItem,Calories,Notes,UserId")] CalorieLogEntry calorieLogEntry)
        {
            if (id != calorieLogEntry.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(calorieLogEntry);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CalorieLogEntryExists(calorieLogEntry.Id))
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
            return View(calorieLogEntry);
        }

        // GET: CalorieLogEntries/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var calorieLogEntry = await _context.CalorieLogEntries
                .FirstOrDefaultAsync(m => m.Id == id);
            if (calorieLogEntry == null)
            {
                return NotFound();
            }

            return View(calorieLogEntry);
        }

        // POST: CalorieLogEntries/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var calorieLogEntry = await _context.CalorieLogEntries.FindAsync(id);
            if (calorieLogEntry != null)
            {
                _context.CalorieLogEntries.Remove(calorieLogEntry);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CalorieLogEntryExists(int id)
        {
            return _context.CalorieLogEntries.Any(e => e.Id == id);
        }
    }
}
