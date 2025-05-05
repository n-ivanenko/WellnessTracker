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
    public class SleepLogEntriesController : Controller
    {
        private readonly AppDbContext _context;

        public SleepLogEntriesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: SleepLogEntries
        public async Task<IActionResult> Index()
        {
            return View(await _context.SleepLogEntries.ToListAsync());
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
        public async Task<IActionResult> Create([Bind("Id,Date,HoursSlept,SleepQuality,Notes")] SleepLogEntry sleepLogEntry)
        {
            if (ModelState.IsValid)
            {
                _context.Add(sleepLogEntry);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(sleepLogEntry);
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
