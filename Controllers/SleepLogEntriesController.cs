using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WellnessTracker.Models;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace WellnessTracker.Controllers
{
    public class SleepLogEntriesController : Controller
    {
        private readonly AppDbContext _context;

        public SleepLogEntriesController(AppDbContext context)
        {
            _context = context;
        }
        private bool UserHasProfile(string userId)
        {
            return _context.UserProfile.Any(up => up.UserId == userId);
        }
        public IActionResult SleepSummary(DateTime? date)
        {
            var selectedDate = date ?? DateTime.Today;

            string? userId = null;
            if (User.Identity.IsAuthenticated)
            {
                userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            }

            var totalSleep = _context.SleepLogEntries
                .Where(s => s.Date.Date == selectedDate.Date && (userId == null || s.UserId == userId))
                .Sum(s => (double?)s.HoursSlept) ?? 0;

            ViewBag.SelectedDate = selectedDate;
            ViewBag.TotalSleepHours = totalSleep;

            return View();
        }

        // GET: Show sleep goal setting page
        [HttpGet]
        public async Task<IActionResult> SetGoal()
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var userGoal = await _context.UserGoals.FirstOrDefaultAsync(g => g.UserId == userId) ?? new UserGoals();
                var profile = await _context.UserProfile.FirstOrDefaultAsync(p => p.UserId == userId);

                int? recommendedSleep = null;

                    if (profile != null)
                    {
                        recommendedSleep = profile.Age < 18 ? 9 : 8;
                    }

            ViewBag.RecommendedSleep = recommendedSleep;

            return View(userGoal);
            }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetGoal(UserGoals userGoal)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            userGoal.UserId = userId;

            ModelState.Remove("UserId");

            if (ModelState.IsValid)
            {
                var existingGoal = await _context.UserGoals.FirstOrDefaultAsync(g => g.UserId == userId);

                if (existingGoal == null)
                {
                    _context.UserGoals.Add(userGoal);
                }
                else
                {
                    existingGoal.SleepGoal = userGoal.SleepGoal;
                    _context.UserGoals.Update(existingGoal);
                }

                await _context.SaveChangesAsync();
                return RedirectToAction("Index", "SleepLogEntries");
            }

            return View(userGoal);
        }


        // GET: SleepLogEntries
        public async Task<IActionResult> Index(int weekOffset = 0)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!UserHasProfile(userId))
            {
                return RedirectToAction("Create", "UserProfile");
            }
            var today = DateTime.Today;
            var weekStart = today.AddDays(weekOffset * 7).AddDays(-(int)today.DayOfWeek + (today.DayOfWeek == DayOfWeek.Sunday ? -6 : 1)); // Monday start
            var weekEnd = weekStart.AddDays(6);

            var entries = await _context.SleepLogEntries
            .Where(e => e.UserId == userId && e.Date.Date >= weekStart && e.Date.Date <= weekEnd)
            .OrderByDescending(e => e.Date)
            .ToListAsync();

            var totalSleptToday = await _context.SleepLogEntries
                .Where(s => s.UserId == userId && s.Date.Date == today)
                .SumAsync(s => (double?)s.HoursSlept) ?? 0;

            var userGoal = await _context.UserGoals.FirstOrDefaultAsync(g => g.UserId == userId);
            var sleepGoal = userGoal?.SleepGoal ?? 0;


            var weekEntries = entries
                .Where(e => e.Date.Date >= weekStart && e.Date.Date <= weekEnd)
                .GroupBy(e => e.Date.Date)
                .ToDictionary(g => g.Key, g => g.Sum(e => e.HoursSlept));

            var weeklyAverage = weekEntries.Values.Count > 0 ? weekEntries.Values.Average() : 0;

            var todaySleep = await _context.SleepLogEntries
                .Where(c => c.UserId == userId && c.Date.Date == today)
                .SumAsync(c => (double?)c.HoursSlept) ?? 0;

            var weeklySleep = new Dictionary<string, double>();
            for (int i = 0; i < 7; i++)
            {
                var date = weekStart.AddDays(i).Date;
                var label = date.ToString("ddd");
                weeklySleep[label] = weekEntries.ContainsKey(date) ? weekEntries[date] : 0;
            }

            ViewBag.WeekOffset = weekOffset;
            ViewBag.WeeklySleep = weeklySleep;
            ViewBag.WeeklyAverageSleep = weeklyAverage;
            ViewBag.TodaySleep = todaySleep;
            ViewBag.SleepPercentage = (userGoal.SleepGoal > 0) ? Math.Min(100, (int)((todaySleep / userGoal.SleepGoal) * 100)) : 0;
            ViewBag.SleepGoal = sleepGoal;

            return View(entries);
        }

        // GET: SleepLogEntries/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var sleepLogEntry = await _context.SleepLogEntries
                .FirstOrDefaultAsync(m => m.Id == id);
            if (sleepLogEntry == null)
            {
                return NotFound();
            }

            return View(sleepLogEntry);
        }

        // GET: SleepLogEntries/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: SleepLogEntries/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SleepLogEntry entry)
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


        // GET: SleepLogEntries/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var sleepLogEntry = await _context.SleepLogEntries.FindAsync(id);
            if (sleepLogEntry == null)
            {
                return NotFound();
            }
            return View(sleepLogEntry);
        }

        // POST: SleepLogEntries/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Date,HoursSlept,SleepQuality,Notes")] SleepLogEntry sleepLogEntry)
        {
            if (id != sleepLogEntry.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(sleepLogEntry);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SleepLogEntryExists(sleepLogEntry.Id))
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
            return View(sleepLogEntry);
        }

        // GET: SleepLogEntries/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var sleepLogEntry = await _context.SleepLogEntries
                .FirstOrDefaultAsync(m => m.Id == id);
            if (sleepLogEntry == null)
            {
                return NotFound();
            }

            return View(sleepLogEntry);
        }

        // POST: SleepLogEntries/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var sleepLogEntry = await _context.SleepLogEntries.FindAsync(id);
            if (sleepLogEntry != null)
            {
                _context.SleepLogEntries.Remove(sleepLogEntry);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool SleepLogEntryExists(int id)
        {
            return _context.SleepLogEntries.Any(e => e.Id == id);
        }
    }
}
