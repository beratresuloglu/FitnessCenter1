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

        // Constructor ile Dependency Injection
        public ServiceController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Service/Index - Tüm hizmetleri listele
        public async Task<IActionResult> Index()
        {
            var serviceList = await _context.Services
                .Include(s => s.GymCenter)
                .Where(s => s.IsActive)
                .ToListAsync();

            return View(serviceList);
        }

        // GET: Service/Create - Sadece Admin
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

        // POST: Service/Create
        // POST: Service/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(Service service)
        {
            // ÇÖZÜM BURADA: GymCenter navigation property'sini doğrulamadan çıkarıyoruz.
            // Çünkü formdan GymCenter nesnesi gelmez, sadece GymCenterId gelir.
            ModelState.Remove("GymCenter");

            if (!ModelState.IsValid)
            {
                // Hata varsa dropdown'ı tekrar doldur
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


        // GET: Service/Edit/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            var service = _context.Services.Find(id); 

            if (service == null)
            {
                return NotFound(); // Hizmet bulunamazsa hata dön
            }

            // 2. Dropdown (Spor Salonu Seçimi) için veriyi tekrar yükle
            // Not: Burası Create metodundaki ile aynı olmalı
            ViewBag.GymCenters = new SelectList(_context.GymCenters, "Id", "Name", service.GymCenterId);

            // 3. Bulunan hizmeti View'a gönder (Bu adım inputların dolmasını sağlar)
            return View(service);
        }

        // POST: Service/Edit/5
        // POST: Service/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,DurationMinutes,Price,GymCenterId")] Service service)
        {
            if (id != service.Id)
            {
                return NotFound();
            }

            // 1. Validasyon Sorununu Çözme
            // Create metodunda olduğu gibi, formdan GymCenter nesnesi gelmediği için
            // doğrulama hatası almamak adına onu ModelState'den siliyoruz.
            ModelState.Remove("GymCenter");

            if (ModelState.IsValid)
            {
                try
                {
                    // 2. Veri Bütünlüğünü Koruma (Best Practice)
                    // Doğrudan _context.Update(service) kullanmak yerine, önce veritabanındaki 
                    // mevcut kaydı çekiyoruz. Böylece formda olmayan (CreatedDate gibi)
                    // alanların sıfırlanmasını engelliyoruz.

                    var existingService = await _context.Services.FindAsync(id);

                    if (existingService == null)
                    {
                        return NotFound();
                    }

                    // Sadece değişen alanları güncelliyoruz
                    existingService.Name = service.Name;
                    existingService.Description = service.Description;
                    existingService.DurationMinutes = service.DurationMinutes;
                    existingService.Price = service.Price;
                    existingService.GymCenterId = service.GymCenterId;

                    // Not: IsActive ve CreatedDate alanlarına dokunmuyoruz,
                    // böylece eski değerlerini koruyorlar.

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

            // Hata durumunda dropdown'ı tekrar doldur
            ViewBag.GymCenters = new SelectList(
                await _context.GymCenters.Where(g => g.IsActive).ToListAsync(),
                "Id",
                "Name",
                service.GymCenterId
            );

            return View(service);
        }

        // GET: Service/Delete/5
        // Silme onay sayfasını getirir
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var service = await _context.Services
                .Include(s => s.GymCenter) // İlişkili veriyi de getir (Ekranda göstermek için)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (service == null)
            {
                return NotFound();
            }

            return View(service);
        }

        // POST: Service/Delete/5
        // Asıl silme (pasife alma) işleminin yapıldığı yer
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // Silinecek kaydı bul
            var service = await _context.Services.FindAsync(id);

            if (service != null)
            {
                // HARD DELETE YERİNE SOFT DELETE YAPIYORUZ
                // _context.Services.Remove(service); // Bu satırı kullanmıyoruz!

                // Durumu False yapıyoruz
                service.IsActive = false;

                // Güncellendi olarak işaretle
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