using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WellnessTracker.Models;

namespace WellnessTracker.Controllers
{
    [Authorize]
    public class UserProfileController : Controller
    {
        private readonly AppDbContext _context;

        public UserProfileController(AppDbContext context)
        {
            _context = context;
        }

        // GET: UserProfiles
        public async Task<IActionResult> Index()
        {
            return View(await _context.UserProfile.ToListAsync());
        }

        // GET: UserProfiles/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userProfile = await _context.UserProfile
                .FirstOrDefaultAsync(m => m.Id == id);
            if (userProfile == null)
            {
                return NotFound();
            }

            return View(userProfile);
        }

        // GET: UserProfiles/Create
        public async Task<IActionResult> Create()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var existingProfile = await _context.UserProfile.FirstOrDefaultAsync(p => p.UserId == userId);
            if (existingProfile != null)
            {
                return RedirectToAction(nameof(Edit), new { id = existingProfile.Id });
            }

            return View();
        }

        // POST: UserProfiles/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UserProfile userProfile)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var existingProfile = await _context.UserProfile.FirstOrDefaultAsync(p => p.UserId == userId);
            if (existingProfile != null)
            {
                return RedirectToAction(nameof(Edit), new { id = existingProfile.Id });
            }

            ModelState.Remove("UserId");

            if (ModelState.IsValid)
            {
                userProfile.UserId = userId;
                _context.Add(userProfile);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index", "Home");
            }
            return View(userProfile);
        }

        // GET: UserProfiles/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var userProfile = await _context.UserProfile.FindAsync(id);

            if (userProfile == null)
            {
                return NotFound();
            }

            return View(userProfile);
        }

        // POST: UserProfiles/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, UserProfile userProfile)
        {
            if (id != userProfile.Id)
                return NotFound();

            ModelState.Remove("UserId");

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var existingProfile = await _context.UserProfile.FindAsync(id);
            if (existingProfile == null || existingProfile.UserId != userId)
                return Unauthorized();

            if (ModelState.IsValid)
            {
                try
                {
                    existingProfile.Name = userProfile.Name;
                    existingProfile.Age = userProfile.Age;
                    existingProfile.HeightIn = userProfile.HeightIn;
                    existingProfile.WeightLb = userProfile.WeightLb;

                    await _context.SaveChangesAsync();
                    return RedirectToAction("Index", "Home");
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UserProfileExists(userProfile.Id))
                        return NotFound();
                    else
                        throw;
                }
            }

            return View(userProfile);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            return Forbid();
        }

        private bool UserProfileExists(int id)
        {
            return _context.UserProfile.Any(e => e.Id == id);
        }
    }
}
