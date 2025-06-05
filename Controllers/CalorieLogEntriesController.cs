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


        // GET: CalorieLogEntries
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var entries = await _context.CalorieLogEntries
                .Where(c => c.UserId == userId)
                .OrderByDescending(c => c.Date)
                .ToListAsync();

            var userGoal = await _context.UserGoals
                .Where(g => g.UserId == userId)
                .Select(g => g.CalorieGoal)
                .FirstOrDefaultAsync();

            ViewBag.CalorieGoal = userGoal;

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
