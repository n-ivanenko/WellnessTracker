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

        public IActionResult CalorieSummary(DateTime? date)
        {
            var selectedDate = date ?? DateTime.Today;

            var userId = User.Identity.IsAuthenticated ? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier).Value : null;

            var totalCalories = _context.CalorieLogEntries
                .Where(c => c.Date.Date == selectedDate.Date && (userId == null || c.UserId == userId))
                .Sum(c => (double?)c.Calories) ?? 0; // nullable to avoid exceptions if no results

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

            return View(userGoal ?? new UserGoal());
        }

        // POST: CalorieLogEntries/SetGoal
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetGoal(UserGoal userGoal)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            userGoal.UserId = userId;

            ModelState.Remove("UserId");

            if (ModelState.IsValid)
            {
                _context.Add(userGoal);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index", "CalorieLogEntries");
            }

            return View(userGoal);
        }
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var today = DateTime.Today;

            var entries = await _context.CalorieLogEntries
                .Where(c => c.UserId == userId && c.Date.Date == today)
                .OrderByDescending(c => c.Date)
                .ToListAsync();

            var userGoal = await _context.UserGoals
                .Where(g => g.UserId == userId)
                .Select(g => g.CalorieGoal)
                .FirstOrDefaultAsync();

            var weekStart = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek + (DateTime.Today.DayOfWeek == DayOfWeek.Sunday ? -6 : 1)); // Monday start
            var weekEnd = weekStart.AddDays(6);

            var weekEntries = entries
                .Where(e => e.Date.Date >= weekStart && e.Date.Date <= weekEnd)
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

            return View(entries);
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
        public async Task<IActionResult> Create([Bind("Id,Date,FoodItem,Calories,Notes,UserId")] CalorieLogEntry calorieLogEntry)
        {
            if (ModelState.IsValid)
            {
                calorieLogEntry.UserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                calorieLogEntry.Date = DateTime.Today;

                _context.Add(calorieLogEntry);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(calorieLogEntry);
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
