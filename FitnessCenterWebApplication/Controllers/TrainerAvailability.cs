using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using FitnessCenterWebApplication.Data;
using FitnessCenterWebApplication.Models.Entities;

namespace FitnessCenterWebApplication.Controllers
{
    public class TrainerAvailabilityController : Controller
    {
        private readonly AppDbContext _context;

        public TrainerAvailabilityController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int trainerId)
        {
            var trainer = await _context.Trainers.FindAsync(trainerId);
            if (trainer == null) return NotFound();

            ViewBag.TrainerName = trainer.FullName;
            ViewBag.TrainerId = trainerId;

            var availabilities = await _context.TrainerAvailabilities
                .Where(a => a.TrainerId == trainerId)
                .OrderBy(a => a.DayOfWeek)
                .ThenBy(a => a.StartTime)
                .ToListAsync();

            return View(availabilities);
        }

        [HttpGet]
        public IActionResult Create(int trainerId)
        {
            ViewBag.Days = new List<SelectListItem>
            {
                new SelectListItem { Value = "1", Text = "Pazartesi" },
                new SelectListItem { Value = "2", Text = "Salı" },
                new SelectListItem { Value = "3", Text = "Çarşamba" },
                new SelectListItem { Value = "4", Text = "Perşembe" },
                new SelectListItem { Value = "5", Text = "Cuma" },
                new SelectListItem { Value = "6", Text = "Cumartesi" },
                new SelectListItem { Value = "0", Text = "Pazar" }
            };

            var model = new TrainerAvailability { TrainerId = trainerId };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TrainerAvailability availability)
        {
            ModelState.Remove("Trainer");

            if (ModelState.IsValid)
            {
                if (availability.EndTime <= availability.StartTime)
                {
                    ModelState.AddModelError("", "Bitiş saati, başlangıç saatinden büyük olmalıdır.");
                }
                else
                {
                    availability.IsActive = true; 
                    _context.Add(availability);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index), new { trainerId = availability.TrainerId });
                }
            }

            ViewBag.Days = new List<SelectListItem>
            {
                new SelectListItem { Value = "1", Text = "Pazartesi" },
                new SelectListItem { Value = "2", Text = "Salı" },
                new SelectListItem { Value = "3", Text = "Çarşamba" },
                new SelectListItem { Value = "4", Text = "Perşembe" },
                new SelectListItem { Value = "5", Text = "Cuma" },
                new SelectListItem { Value = "6", Text = "Cumartesi" },
                new SelectListItem { Value = "0", Text = "Pazar" }
            };

            return View(availability);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var availability = await _context.TrainerAvailabilities.FindAsync(id);
            if (availability != null)
            {
                int trainerId = availability.TrainerId;
                _context.TrainerAvailabilities.Remove(availability);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index), new { trainerId = trainerId });
            }
            return NotFound();
        }
    }
}