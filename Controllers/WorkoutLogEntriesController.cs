using System;
using System.Collections.Generic;
using System.Linq;
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


        // GET: WorkoutEntries
        public async Task<IActionResult> Index()
        {
            return View(await _context.WorkoutLogEntries.ToListAsync());
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
