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
    public class HabitEntriesController : Controller
    {
        private readonly AppDbContext _context;

        public HabitEntriesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: HabitEntries
        public async Task<IActionResult> Index()
        {
            return View(await _context.HabitEntries.ToListAsync());
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
        public async Task<IActionResult> Create([Bind("Id,HabitName,StartDate,TargetDate,IsCompleted,Notes")] HabitEntry habitEntry)
        {
            if (ModelState.IsValid)
            {
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
        public async Task<IActionResult> Edit(int id, [Bind("Id,HabitName,StartDate,TargetDate,IsCompleted,Notes")] HabitEntry habitEntry)
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
