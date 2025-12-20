using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Net.NetworkInformation;
using FitnessCenterWebApplication.Models.Entities;
using FitnessCenterWebApplication.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;


namespace FitnessCenterWebApplication.Controllers
{
    public class ServiceController : Controller
    {
        private readonly AppDbContext _context;

        public ServiceController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var serviceList = await _context.Services
                .Include(s => s.GymCenter)
                .Where(s => s.IsActive)
                .ToListAsync();

            return View(serviceList);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create()
        {
            ViewBag.GymCenters = new SelectList(
                await _context.GymCenters.Where(g => g.IsActive).ToListAsync(),
                "Id",
                "Name"
            );

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(Service service)
        {
            ModelState.Remove("GymCenter");

            if (!ModelState.IsValid)
            {
                ViewBag.GymCenters = new SelectList(
                    await _context.GymCenters
                        .Where(g => g.IsActive)
                        .ToListAsync(),
                    "Id",
                    "Name",
                    service.GymCenterId
                );

                return View(service);
            }

            service.IsActive = true;
            service.CreatedDate = DateTime.Now;

            _context.Services.Add(service);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Hizmet başarıyla eklendi!";
            return RedirectToAction(nameof(Index));
        }


        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            var service = _context.Services.Find(id); 

            if (service == null)
            {
                return NotFound();
            }

            ViewBag.GymCenters = new SelectList(_context.GymCenters, "Id", "Name", service.GymCenterId);

            return View(service);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,DurationMinutes,Price,GymCenterId")] Service service)
        {
            if (id != service.Id)
            {
                return NotFound();
            }

            ModelState.Remove("GymCenter");

            if (ModelState.IsValid)
            {
                try
                {

                    var existingService = await _context.Services.FindAsync(id);

                    if (existingService == null)
                    {
                        return NotFound();
                    }

                    existingService.Name = service.Name;
                    existingService.Description = service.Description;
                    existingService.DurationMinutes = service.DurationMinutes;
                    existingService.Price = service.Price;
                    existingService.GymCenterId = service.GymCenterId;


                    _context.Update(existingService);
                    await _context.SaveChangesAsync();

                    TempData["Success"] = "Hizmet başarıyla güncellendi!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ServiceExists(service.Id))
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
                service.GymCenterId
            );

            return View(service);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var service = await _context.Services
                .Include(s => s.GymCenter) 
                .FirstOrDefaultAsync(m => m.Id == id);

            if (service == null)
            {
                return NotFound();
            }

            return View(service);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var service = await _context.Services.FindAsync(id);

            if (service != null)
            {
                service.IsActive = false;

                _context.Update(service);

                await _context.SaveChangesAsync();
                TempData["Success"] = "Hizmet başarıyla silindi (pasife alındı)!";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool ServiceExists(int id)
        {
            return _context.Services.Any(e => e.Id == id);
        }
    }
}