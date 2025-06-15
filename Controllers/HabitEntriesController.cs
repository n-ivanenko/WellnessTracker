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
    public class HabitEntriesController : Controller
    {
        private readonly AppDbContext _context;

        public HabitEntriesController(AppDbContext context)
        {
            _context = context;
        }
        private bool UserHasProfile(string userId)
        {
            return _context.UserProfile.Any(up => up.UserId == userId);
        }


        // GET: HabitEntries
        public async Task<IActionResult> Index(int weekOffset = 0)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var today = DateTime.Today;
            var baseDate = today.AddDays(weekOffset * 7);
            var weekStart = baseDate.AddDays(-(int)baseDate.DayOfWeek + (baseDate.DayOfWeek == DayOfWeek.Sunday ? -6 : 1)); // Monday
            var weekEnd = weekStart.AddDays(6);

            var weekDates = Enumerable.Range(0, 7)
                .Select(offset => weekStart.AddDays(offset))
                .ToList();

            var habits = await _context.HabitEntries
                .Where(h => h.UserId == userId)
                .ToListAsync();

            var habitIds = habits.Select(h => h.Id).ToList();

            var completions = await _context.HabitCompletions
                .Where(c => habitIds.Contains(c.HabitEntryId)
                    && c.UserId == userId
                    && c.Date.Date >= weekStart
                    && c.Date.Date <= weekEnd)
                .ToListAsync();

            ViewBag.WeekDates = weekDates;
            ViewBag.Completions = completions;
            ViewBag.WeekOffset = weekOffset;

            return View(habits);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitHabitCompletions(List<string> completions)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var submitted = completions
                .Select(c => c.Split('|'))
                .Select(parts => new
                {
                    HabitEntryId = int.Parse(parts[0]),
                    Date = DateTime.Parse(parts[1])
                })
                .ToList();

            DateTime startOfWeek = DateTime.Today;
            while (startOfWeek.DayOfWeek != DayOfWeek.Monday)
            {
                startOfWeek = startOfWeek.AddDays(-1);
            }

            var endOfWeek = startOfWeek.AddDays(7);

            var userCompletions = await _context.HabitCompletions
                .Where(c => c.UserId == userId &&
                            c.Date >= startOfWeek &&
                            c.Date < endOfWeek)
                .ToListAsync();

            foreach (var habit in submitted)
            {
                var existing = userCompletions.FirstOrDefault(c =>
                    c.HabitEntryId == habit.HabitEntryId &&
                    c.Date.Date == habit.Date.Date);

                if (existing == null)
                {
                    _context.HabitCompletions.Add(new HabitCompletion
                    {
                        HabitEntryId = habit.HabitEntryId,
                        Date = habit.Date.Date,
                        IsCompleted = true,
                        UserId = userId
                    });
                }
                else if (!existing.IsCompleted)
                {
                    existing.IsCompleted = true;
                    _context.Update(existing);
                }
            }

            foreach (var existing in userCompletions)
            {
                bool stillChecked = submitted.Any(s =>
                    s.HabitEntryId == existing.HabitEntryId &&
                    s.Date.Date == existing.Date.Date);

                if (!stillChecked && existing.IsCompleted)
                {
                    existing.IsCompleted = false;
                    _context.Update(existing);
                }
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> All()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var habits = await _context.HabitEntries
                .Where(h => h.UserId == userId)
                .ToListAsync();

            return View(habits);
        }

        // GET: HabitEntries/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var habitEntry = await _context.HabitEntries
                .FirstOrDefaultAsync(m => m.Id == id);
            if (habitEntry == null)
            {
                return NotFound();
            }

            return View(habitEntry);
        }

        // GET: HabitEntries/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: HabitEntries/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,HabitName,Notes")] HabitEntry habitEntry)
        {
            if (ModelState.IsValid)
            {
                habitEntry.UserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                _context.Add(habitEntry);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(habitEntry);
        }

        // GET: HabitEntries/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var habitEntry = await _context.HabitEntries.FindAsync(id);
            if (habitEntry == null)
            {
                return NotFound();
            }
            return View(habitEntry);
        }

        // POST: HabitEntries/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,HabitName,Notes")] HabitEntry habitEntry)
        {
            if (id != habitEntry.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(habitEntry);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!HabitEntryExists(habitEntry.Id))
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
            return View(habitEntry);
        }

        // GET: HabitEntries/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var habitEntry = await _context.HabitEntries
                .FirstOrDefaultAsync(m => m.Id == id);
            if (habitEntry == null)
            {
                return NotFound();
            }

            return View(habitEntry);
        }

        // POST: HabitEntries/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var habitEntry = await _context.HabitEntries.FindAsync(id);
            if (habitEntry != null)
            {
                _context.HabitEntries.Remove(habitEntry);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool HabitEntryExists(int id)
        {
            return _context.HabitEntries.Any(e => e.Id == id);
        }
    }
}
