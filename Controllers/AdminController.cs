using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WellnessTracker.Models;
using System.Threading.Tasks;
using System.Linq;
using System.Security.Claims;

namespace WellnessTracker.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly AppDbContext _context;

        public AdminController(UserManager<IdentityUser> userManager, AppDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }
        public async Task<IActionResult> Index()
        {
            var users = await _userManager.Users.ToListAsync();
            var model = new List<AdminUserDetailsViewModel>();

            foreach (var user in users)
            {
                if (await _userManager.IsInRoleAsync(user, "Admin"))
                {
                    continue;
                }

                var profile = await _context.UserProfile.FirstOrDefaultAsync(p => p.UserId == user.Id);
                var moods = await _context.MoodEntries.Where(m => m.UserId == user.Id).ToListAsync();
                var calories = await _context.CalorieLogEntries.Where(c => c.UserId == user.Id).ToListAsync();
                var sleeps = await _context.SleepLogEntries.Where(s => s.UserId == user.Id).ToListAsync();
                var workouts = await _context.WorkoutLogEntries.Where(w => w.UserId == user.Id).ToListAsync();
                var habitEntries = await _context.HabitEntries.Where(h => h.UserId == user.Id).ToListAsync();
                var habitCompletions = await _context.HabitCompletions.Where(hc => hc.UserId == user.Id).ToListAsync();

                model.Add(new AdminUserDetailsViewModel
                {
                    User = user,
                    Profile = profile,
                    MoodEntries = moods,
                    CalorieLogs = calories,
                    SleepLogs = sleeps,
                    WorkoutLogs = workouts,
                    HabitEntries = habitEntries,
                    HabitCompletions = habitCompletions
                });
            }

            return View(model);
        }
        public async Task<IActionResult> Details(string id)
        {
            if (id == null) return NotFound();

            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var profile = await _context.UserProfile.FirstOrDefaultAsync(p => p.UserId == user.Id);

            var calorieLogs = await _context.CalorieLogEntries
                .Where(c => c.UserId == user.Id)
                .ToListAsync();
            var moodEntries = await _context.MoodEntries.Where(m => m.UserId == user.Id).ToListAsync();
            var sleepLogs = await _context.SleepLogEntries.Where(s => s.UserId == user.Id).ToListAsync();
            var workoutLogs = await _context.WorkoutLogEntries.Where(w => w.UserId == user.Id).ToListAsync();
            var habitEntries = await _context.HabitEntries.Where(h => h.UserId == user.Id).ToListAsync();
            var habitCompletions = await _context.HabitCompletions.Where(hc => hc.UserId == user.Id).ToListAsync();


            var model = new AdminUserDetailsViewModel
            {
                User = user,
                Profile = profile,
                CalorieLogs = calorieLogs,
                MoodEntries = moodEntries,
                SleepLogs = sleepLogs,
                WorkoutLogs = workoutLogs,
                HabitEntries = habitEntries,
                HabitCompletions = habitCompletions
            };

            return View(model);
        }

        // Edit User email and username

        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null) return NotFound();

            var profile = await _context.UserProfile.FirstOrDefaultAsync(p => p.UserId == id);
            if (profile == null) return NotFound();

            return View(profile);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(UserProfile profile)
        {
            ModelState.Remove("Id");
            if (!ModelState.IsValid)
            {
                return View(profile);
            }

            var existingProfile = await _context.UserProfile.FirstOrDefaultAsync(p => p.UserId == profile.UserId);
            if (existingProfile == null)
            {
                return NotFound();
            }
            existingProfile.Name = profile.Name;
            existingProfile.Age = profile.Age;
            existingProfile.HeightIn = profile.HeightIn;
            existingProfile.WeightLb = profile.WeightLb;

            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }


        // Reset User Password
        public IActionResult ResetPassword(string id)
        {
            if (id == null) return NotFound();

            var model = new ResetPasswordViewModel { UserId = id };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _userManager.FindByIdAsync(model.UserId);
            if (user == null) return NotFound();

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, token, model.NewPassword);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError("", error.Description);
                return View(model);
            }

            return RedirectToAction(nameof(Index));
        }

        // Delete User

        [HttpGet]
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null) return NotFound();

            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            return View(user); 
        }


        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var profile = await _context.UserProfile.FirstOrDefaultAsync(p => p.UserId == id);
            if (profile != null) _context.UserProfile.Remove(profile);

            var moodLogs = _context.MoodEntries.Where(e => e.UserId == id);
            _context.MoodEntries.RemoveRange(moodLogs);

            var calorieLogs = _context.CalorieLogEntries.Where(e => e.UserId == id);
            _context.CalorieLogEntries.RemoveRange(calorieLogs);

            var sleepLogs = _context.SleepLogEntries.Where(e => e.UserId == id);
            _context.SleepLogEntries.RemoveRange(sleepLogs);

            var workoutLogs = _context.WorkoutLogEntries.Where(e => e.UserId == id);
            _context.WorkoutLogEntries.RemoveRange(workoutLogs);

            var habitLogs = _context.HabitEntries.Where(e => e.UserId == id);
            _context.HabitEntries.RemoveRange(habitLogs);

            await _context.SaveChangesAsync();

            await _userManager.DeleteAsync(user);

            return RedirectToAction("Index");
        }

        // Mood Entries

        [HttpGet("Admin/UserMoodEntries/{userId}")]
        public async Task<IActionResult> UserMoodEntries(string userId)
        {
            if (userId == null) return NotFound();

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();

            var moodEntries = await _context.MoodEntries
                .Where(e => e.UserId == userId)
                .OrderByDescending(e => e.Date)
                .ToListAsync();

            ViewBag.UserId = userId;
            ViewBag.UserEmail = user.Email;

            return View(moodEntries);
        }

        public async Task<IActionResult> EditMoodEntry(int id)
        {
            var entry = await _context.MoodEntries.FindAsync(id);
            if (entry == null) return NotFound();

            return View(entry);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditMoodEntry(int id, MoodEntry updatedEntry)
        {
            if (id != updatedEntry.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(updatedEntry);
                    await _context.SaveChangesAsync();
                    return RedirectToAction("UserMoodEntries", new { userId = updatedEntry.UserId });
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.MoodEntries.Any(e => e.Id == id))
                        return NotFound();
                    else
                        throw;
                }
            }

            return View(updatedEntry);
        }

        public async Task<IActionResult> DeleteMoodEntry(int id)
        {
            var entry = await _context.MoodEntries.FindAsync(id);
            if (entry == null) return NotFound();

            return View(entry);
        }

        [HttpPost, ActionName("DeleteMoodEntry")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteMoodEntryConfirmed(int id)
        {
            var entry = await _context.MoodEntries.FindAsync(id);
            if (entry != null)
            {
                _context.MoodEntries.Remove(entry);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("UserMoodEntries", new { userId = entry?.UserId });
        }

        // Calorie Entries

        [HttpGet("Admin/UserCalorieEntries/{userId}")]
        public async Task<IActionResult> UserCalorieEntries(string userId)
        {
            if (userId == null) return NotFound();

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();

            var entries = await _context.CalorieLogEntries
                .Where(c => c.UserId == userId)
                .OrderByDescending(c => c.Date)
                .ToListAsync();

            ViewBag.UserId = userId;

            return View(entries);
        }
        public async Task<IActionResult> EditCalorieEntry(int id)
        {
            var entry = await _context.CalorieLogEntries.FindAsync(id);
            if (entry == null) return NotFound();

            return View(entry);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditCalorieEntry(CalorieLogEntry entry)
        {
            if (ModelState.IsValid)
            {
                _context.Update(entry);
                await _context.SaveChangesAsync();
                return RedirectToAction("UserCalorieEntries", new { userId = entry.UserId });
            }

            return View(entry);
        }
        public async Task<IActionResult> DeleteCalorieEntry(int id)
        {
            var entry = await _context.CalorieLogEntries.FindAsync(id);
            if (entry == null) return NotFound();

            return View(entry);
        }

        [HttpPost, ActionName("DeleteCalorieEntry")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCalorieEntryConfirmed(int id)
        {
            var entry = await _context.CalorieLogEntries.FindAsync(id);
            var userId = entry.UserId;

            _context.CalorieLogEntries.Remove(entry);
            await _context.SaveChangesAsync();

            return RedirectToAction("UserCalorieEntries", new { userId });
        }

        // Sleep Entries

        [HttpGet("Admin/UserSleepEntries/{userId}")]
        public async Task<IActionResult> UserSleepEntries(string userId)
        {
            if (userId == null) return NotFound();

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();

            var entries = await _context.SleepLogEntries
                .Where(c => c.UserId == userId)
                .OrderByDescending(c => c.Date)
                .ToListAsync();

            ViewBag.UserId = userId;

            return View(entries);
        }
        public async Task<IActionResult> EditSleepEntry(int id)
        {
            var entry = await _context.SleepLogEntries.FindAsync(id);
            if (entry == null) return NotFound();

            return View(entry);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditSleepEntry(SleepLogEntry entry)
        {
            if (ModelState.IsValid)
            {
                _context.Update(entry);
                await _context.SaveChangesAsync();
                return RedirectToAction("UserSleepEntries", new { userId = entry.UserId });
            }

            return View(entry);
        }
        public async Task<IActionResult> DeleteSleepEntry(int id)
        {
            var entry = await _context.SleepLogEntries.FindAsync(id);
            if (entry == null) return NotFound();

            return View(entry);
        }

        [HttpPost, ActionName("DeleteSleepEntry")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteSleepEntryConfirmed(int id)
        {
            var entry = await _context.SleepLogEntries.FindAsync(id);
            var userId = entry.UserId;

            _context.SleepLogEntries.Remove(entry);
            await _context.SaveChangesAsync();

            return RedirectToAction("UserSleepEntries", new { userId });
        }

        // Workout Entries

        [HttpGet("Admin/UserWorkoutEntries/{userId}")]
        public async Task<IActionResult> UserWorkoutEntries(string userId)
        {
            if (userId == null) return NotFound();

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();

            var entries = await _context.WorkoutLogEntries
                .Where(c => c.UserId == userId)
                .OrderByDescending(c => c.Date)
                .ToListAsync();

            ViewBag.UserId = userId;

            return View(entries);
        }
        public async Task<IActionResult> EditWorkoutEntry(int id)
        {
            var entry = await _context.WorkoutLogEntries.FindAsync(id);
            if (entry == null) return NotFound();

            return View(entry);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditWorkoutEntry(WorkoutLogEntry entry)
        {
            if (ModelState.IsValid)
            {
                _context.Update(entry);
                await _context.SaveChangesAsync();
                return RedirectToAction("UserWorkoutEntries", new { userId = entry.UserId });
            }

            return View(entry);
        }
        public async Task<IActionResult> DeleteWorkoutEntry(int id)
        {
            var entry = await _context.WorkoutLogEntries.FindAsync(id);
            if (entry == null) return NotFound();

            return View(entry);
        }

        [HttpPost, ActionName("DeleteWorkoutEntry")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteWorkoutEntryConfirmed(int id)
        {
            var entry = await _context.WorkoutLogEntries.FindAsync(id);
            var userId = entry.UserId;

            _context.WorkoutLogEntries.Remove(entry);
            await _context.SaveChangesAsync();

            return RedirectToAction("UserWorkoutEntries", new { userId });
        }

        // Habit entries

        [HttpGet("Admin/UserHabits/{userId}")]
        public async Task<IActionResult> UserHabits(string userId)
        {
            if (userId == null) return NotFound();

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();

            var habitEntries = await _context.HabitEntries
                .Where(h => h.UserId == userId)
                .ToListAsync();

            var habitCompletions = await _context.HabitCompletions
                .Where(c => c.UserId == userId)
                .ToListAsync();

            ViewBag.UserEmail = user.Email;
            ViewBag.HabitCompletions = habitCompletions;

            return View(habitEntries);
        }

        [HttpPost]
        public async Task<IActionResult> ToggleCompletion(int id)
        {
            var completion = await _context.HabitCompletions.FindAsync(id);
            if (completion == null) return NotFound();

            completion.IsCompleted = !completion.IsCompleted;
            await _context.SaveChangesAsync();

            return RedirectToAction("UserHabits", new { userId = completion.UserId });
        }

        public async Task<IActionResult> EditHabit(int id)
        {
            var habit = await _context.HabitEntries.FindAsync(id);
            if (habit == null) return NotFound();
            return View("EditUserHabits", habit);
        }

        [HttpPost]
        public async Task<IActionResult> EditHabit(HabitEntry habit)
        {
            var existing = await _context.HabitEntries.FindAsync(habit.Id);
            if (existing == null) return NotFound();

            existing.HabitName = habit.HabitName;
            existing.Notes = habit.Notes;
            await _context.SaveChangesAsync();

            return RedirectToAction("UserHabits", new { userId = habit.UserId });
        }

        public async Task<IActionResult> DeleteHabit(int id)
        {
            var habit = await _context.HabitEntries.FindAsync(id);
            if (habit == null) return NotFound();

            string userId = habit.UserId!;
            _context.HabitEntries.Remove(habit);
            await _context.SaveChangesAsync();

            return RedirectToAction("DeleteUserserHabits", new { userId = userId });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteCompletion(int id)
        {
            var completion = await _context.HabitCompletions.FindAsync(id);
            if (completion == null) return NotFound();

            string userId = completion.UserId!;
            _context.HabitCompletions.Remove(completion);
            await _context.SaveChangesAsync();

            return RedirectToAction("UserHabits", new { userId = userId });
        }

    }
}
