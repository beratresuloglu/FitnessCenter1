using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using FitnessCenterWebApplication.Data;
using FitnessCenterWebApplication.Models.Entities;

namespace FitnessCenterWebApplication.Controllers
{
    public class TrainerController : Controller
    {
        private readonly AppDbContext _context;

        public TrainerController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var trainers = await _context.Trainers
                .Include(t => t.GymCenter)
                .Where(t => t.IsActive) 
                .ToListAsync();

            return View(trainers);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create()
        {
            ViewBag.GymCenters = new SelectList(
                await _context.GymCenters.Where(g => g.IsActive).ToListAsync(),
                "Id",
                "Name"
            );

            ViewBag.Services = await _context.Services.Where(s => s.IsActive).ToListAsync();

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(Trainer trainer, int[] selectedServiceIds)
        {
            ModelState.Remove("GymCenter");
            ModelState.Remove("User");
            ModelState.Remove("TrainerServices");
            ModelState.Remove("Availabilities");
            ModelState.Remove("Appointments");

            if (ModelState.IsValid)
            {
                trainer.IsActive = true;
                trainer.CreatedDate = DateTime.Now;
                if (trainer.HireDate == default) trainer.HireDate = DateTime.Now;

                _context.Add(trainer);
                await _context.SaveChangesAsync();

                if (selectedServiceIds != null && selectedServiceIds.Length > 0)
                {
                    foreach (var serviceId in selectedServiceIds)
                    {
                        var trainerService = new TrainerService
                        {
                            TrainerId = trainer.Id,
                            ServiceId = serviceId,
                            IsActive = true,
                            AssignedDate = DateTime.Now
                        };
                        _context.TrainerServices.Add(trainerService);
                    }
                    await _context.SaveChangesAsync();
                }

                TempData["Success"] = "Eğitmen ve hizmetleri başarıyla eklendi!";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.GymCenters = new SelectList(await _context.GymCenters.Where(g => g.IsActive).ToListAsync(), "Id", "Name", trainer.GymCenterId);

            ViewBag.Services = await _context.Services.Where(s => s.IsActive).ToListAsync();

            return View(trainer);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var trainer = await _context.Trainers.FindAsync(id);

            if (trainer == null)
            {
                return NotFound();
            }

            ViewBag.GymCenters = new SelectList(
                await _context.GymCenters.Where(g => g.IsActive).ToListAsync(),
                "Id",
                "Name",
                trainer.GymCenterId
            );

            return View(trainer);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, Trainer trainer)
        {
            if (id != trainer.Id)
            {
                return NotFound();
            }

            ModelState.Remove("GymCenter");
            ModelState.Remove("User");
            ModelState.Remove("TrainerServices");
            ModelState.Remove("Availabilities");
            ModelState.Remove("Appointments");

            if (ModelState.IsValid)
            {
                try
                {
                    var existingTrainer = await _context.Trainers.FindAsync(id);

                    if (existingTrainer == null)
                    {
                        return NotFound();
                    }

                    existingTrainer.FirstName = trainer.FirstName;
                    existingTrainer.LastName = trainer.LastName;
                    existingTrainer.Phone = trainer.Phone;
                    existingTrainer.Email = trainer.Email;
                    existingTrainer.Specialization = trainer.Specialization;
                    existingTrainer.Bio = trainer.Bio;
                    existingTrainer.ExperienceYears = trainer.ExperienceYears;
                    existingTrainer.GymCenterId = trainer.GymCenterId;

                    existingTrainer.ProfileImageUrl = trainer.ProfileImageUrl;


                    _context.Update(existingTrainer);
                    await _context.SaveChangesAsync();

                    TempData["Success"] = "Eğitmen bilgileri güncellendi!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TrainerExists(trainer.Id))
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

            ViewBag.GymCenters = new SelectList(
                await _context.GymCenters.Where(g => g.IsActive).ToListAsync(),
                "Id",
                "Name",
                trainer.GymCenterId
            );

            return View(trainer);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var trainer = await _context.Trainers
                .Include(t => t.GymCenter)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (trainer == null)
            {
                return NotFound();
            }

            return View(trainer);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var trainer = await _context.Trainers.FindAsync(id);

            if (trainer != null)
            {
                trainer.IsActive = false;

                _context.Update(trainer);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Eğitmen başarıyla silindi (pasife alındı)!";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool TrainerExists(int id)
        {
            return _context.Trainers.Any(e => e.Id == id);
        }
    }
}