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
    public class MoodEntriesController : Controller
    {
        private readonly AppDbContext _context;

        public MoodEntriesController(AppDbContext context)
        {
            _context = context;
        }

        private bool UserHasProfile(string userId)
        {
            return _context.UserProfile.Any(up => up.UserId == userId);
        }

        [HttpGet]
        public async Task<IActionResult> MoodGraphData(int weekOffset = 0)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var today = DateTime.Today;

            var startOfWeek = today.AddDays(-(int)(today.DayOfWeek == DayOfWeek.Sunday ? 6 : today.DayOfWeek - DayOfWeek.Monday))
                                   .AddDays(weekOffset * 7);
            var endOfWeek = startOfWeek.AddDays(6);

            var moodData = await _context.MoodEntries
                .Where(m => m.UserId == userId && m.Date.Date >= startOfWeek.Date && m.Date.Date <= endOfWeek.Date)
                .OrderBy(m => m.Date)
                .Select(m => new
                {
                    date = m.Date.ToString("MM/dd"),
                    moodRating = m.MoodRating
                })
                .ToListAsync();

            return Json(moodData);
        }

        // GET: MoodEntries
        public async Task<IActionResult> Index(int weekOffset = 0)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!UserHasProfile(userId))
            {
                return RedirectToAction("Create", "UserProfile");
            }

            var today = DateTime.Today;
            var startOfWeek = today.AddDays(-(int)(today.DayOfWeek == DayOfWeek.Sunday ? 6 : today.DayOfWeek - DayOfWeek.Monday))
                                   .AddDays(weekOffset * 7);
            var endOfWeek = startOfWeek.AddDays(6);

            var entries = await _context.MoodEntries
                .Where(m => m.UserId == userId && m.Date.Date >= startOfWeek && m.Date.Date <= endOfWeek)
                .OrderBy(m => m.Date)
                .ToListAsync();

            ViewBag.WeekOffset = weekOffset;
            ViewBag.StartOfWeek = startOfWeek;
            ViewBag.EndOfWeek = endOfWeek;

            return View(entries);
        }


        // GET: MoodEntries/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var moodEntry = await _context.MoodEntries
                .FirstOrDefaultAsync(m => m.Id == id);
            if (moodEntry == null)
            {
                return NotFound();
            }

            return View(moodEntry);
        }

        // GET: MoodEntries/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: MoodEntries/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Date,MoodRating,Notes")] MoodEntry moodEntry)
        {
            if (ModelState.IsValid)
            {
                moodEntry.UserId = User.FindFirstValue(ClaimTypes.NameIdentifier); 
                _context.Add(moodEntry);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(moodEntry);
        }


        // GET: MoodEntries/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var moodEntry = await _context.MoodEntries.FindAsync(id);
            if (moodEntry == null)
            {
                return NotFound();
            }
            return View(moodEntry);
        }

        // POST: MoodEntries/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Date,MoodRating,Notes")] MoodEntry moodEntry)
        {
            if (id != moodEntry.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(moodEntry);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MoodEntryExists(moodEntry.Id))
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
            return View(moodEntry);
        }

        // GET: MoodEntries/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var moodEntry = await _context.MoodEntries
                .FirstOrDefaultAsync(m => m.Id == id);
            if (moodEntry == null)
            {
                return NotFound();
            }

            return View(moodEntry);
        }

        // POST: MoodEntries/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var moodEntry = await _context.MoodEntries.FindAsync(id);
            if (moodEntry != null)
            {
                _context.MoodEntries.Remove(moodEntry);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool MoodEntryExists(int id)
        {
            return _context.MoodEntries.Any(e => e.Id == id);
        }
    }
}
