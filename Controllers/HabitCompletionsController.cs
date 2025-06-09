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
    public class HabitCompletionsController : Controller
    {
        private readonly AppDbContext _context;

        public HabitCompletionsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> MarkComplete(int habitEntryId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var today = DateTime.Today;

            var existing = await _context.HabitCompletions
                .FirstOrDefaultAsync(hc => hc.HabitEntryId == habitEntryId
                                           && hc.UserId == userId
                                           && hc.Date == today);

            if (existing == null)
            {
                var completion = new HabitCompletion
                {
                    HabitEntryId = habitEntryId,
                    Date = today,
                    IsCompleted = true,
                    UserId = userId
                };
                _context.HabitCompletions.Add(completion);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Index", "HabitEntries");
        }
        // GET: HabitCompletions
        public async Task<IActionResult> Index()
        {
            var appDbContext = _context.HabitCompletions.Include(h => h.HabitEntry);
            return View(await appDbContext.ToListAsync());
        }

        // GET: HabitCompletions/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var habitCompletion = await _context.HabitCompletions
                .Include(h => h.HabitEntry)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (habitCompletion == null)
            {
                return NotFound();
            }

            return View(habitCompletion);
        }

        // GET: HabitCompletions/Create
        public IActionResult Create()
        {
            ViewData["HabitEntryId"] = new SelectList(_context.HabitEntries, "Id", "HabitName");
            return View();
        }

        // POST: HabitCompletions/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,HabitEntryId,Date,IsCompleted")] HabitCompletion habitCompletion)
        {
            if (ModelState.IsValid)
            {
                _context.Add(habitCompletion);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["HabitEntryId"] = new SelectList(_context.HabitEntries, "Id", "HabitName", habitCompletion.HabitEntryId);
            return View(habitCompletion);
        }

        // GET: HabitCompletions/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var habitCompletion = await _context.HabitCompletions.FindAsync(id);
            if (habitCompletion == null)
            {
                return NotFound();
            }
            ViewData["HabitEntryId"] = new SelectList(_context.HabitEntries, "Id", "HabitName", habitCompletion.HabitEntryId);
            return View(habitCompletion);
        }

        // POST: HabitCompletions/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,HabitEntryId,Date,IsCompleted")] HabitCompletion habitCompletion)
        {
            if (id != habitCompletion.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(habitCompletion);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!HabitCompletionExists(habitCompletion.Id))
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
            ViewData["HabitEntryId"] = new SelectList(_context.HabitEntries, "Id", "HabitName", habitCompletion.HabitEntryId);
            return View(habitCompletion);
        }

        // GET: HabitCompletions/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var habitCompletion = await _context.HabitCompletions
                .Include(h => h.HabitEntry)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (habitCompletion == null)
            {
                return NotFound();
            }

            return View(habitCompletion);
        }

        // POST: HabitCompletions/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var habitCompletion = await _context.HabitCompletions.FindAsync(id);
            if (habitCompletion != null)
            {
                _context.HabitCompletions.Remove(habitCompletion);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool HabitCompletionExists(int id)
        {
            return _context.HabitCompletions.Any(e => e.Id == id);
        }
    }
}
