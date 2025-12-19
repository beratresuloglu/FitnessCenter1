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

        // GET: Trainer/Index
        public async Task<IActionResult> Index()
        {
            // Trainer'ları çekerken bağlı olduğu GymCenter'ı da dahil ediyoruz (Include)
            var trainers = await _context.Trainers
                .Include(t => t.GymCenter)
                .Where(t => t.IsActive) // Sadece aktif olanları listele
                .ToListAsync();

            return View(trainers);
        }

        // GET: Trainer/Create - Sadece Admin
        // GET: Trainer/Create - Sadece Admin
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create()
        {
            // 1. GymCenter Listesi (Dropdown için)
            ViewBag.GymCenters = new SelectList(
                await _context.GymCenters.Where(g => g.IsActive).ToListAsync(),
                "Id",
                "Name"
            );

            // 2. YENİ: Hizmet Listesi (Checkboxlar için)
            // Aktif olan tüm hizmetleri çekip View'a gönderiyoruz
            ViewBag.Services = await _context.Services.Where(s => s.IsActive).ToListAsync();

            return View();
        }

        // POST: Trainer/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        // YENİ: int[] selectedServiceIds parametresini ekledik (Seçilen kutucukların ID'leri buraya gelecek)
        public async Task<IActionResult> Create(Trainer trainer, int[] selectedServiceIds)
        {
            // Validasyon temizliği
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

                // 1. Önce Eğitmeni Kaydet (ID oluşması için)
                _context.Add(trainer);
                await _context.SaveChangesAsync();

                // 2. YENİ: Seçilen Hizmetleri TrainerService Tablosuna Ekle
                if (selectedServiceIds != null && selectedServiceIds.Length > 0)
                {
                    foreach (var serviceId in selectedServiceIds)
                    {
                        var trainerService = new TrainerService
                        {
                            TrainerId = trainer.Id, // Yeni oluşan eğitmen ID'si
                            ServiceId = serviceId,
                            IsActive = true,
                            AssignedDate = DateTime.Now
                        };
                        _context.TrainerServices.Add(trainerService);
                    }
                    // Değişiklikleri kaydet
                    await _context.SaveChangesAsync();
                }

                TempData["Success"] = "Eğitmen ve hizmetleri başarıyla eklendi!";
                return RedirectToAction(nameof(Index));
            }

            // Hata durumunda listeleri tekrar doldur
            ViewBag.GymCenters = new SelectList(await _context.GymCenters.Where(g => g.IsActive).ToListAsync(), "Id", "Name", trainer.GymCenterId);

            // Hizmet listesini de tekrar gönder (Hata alınırsa kutucuklar kaybolmasın)
            ViewBag.Services = await _context.Services.Where(s => s.IsActive).ToListAsync();

            return View(trainer);
        }

        // GET: Trainer/Edit/5
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

            // Dropdown'ı mevcut seçili GymCenter ile doldur
            ViewBag.GymCenters = new SelectList(
                await _context.GymCenters.Where(g => g.IsActive).ToListAsync(),
                "Id",
                "Name",
                trainer.GymCenterId
            );

            return View(trainer);
        }

        // POST: Trainer/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, Trainer trainer)
        {
            if (id != trainer.Id)
            {
                return NotFound();
            }

            // Validasyon temizliği
            ModelState.Remove("GymCenter");
            ModelState.Remove("User");
            ModelState.Remove("TrainerServices");
            ModelState.Remove("Availabilities");
            ModelState.Remove("Appointments");

            if (ModelState.IsValid)
            {
                try
                {
                    // Best Practice: Önce mevcut veriyi çekiyoruz
                    var existingTrainer = await _context.Trainers.FindAsync(id);

                    if (existingTrainer == null)
                    {
                        return NotFound();
                    }

                    // Sadece formdan değiştirilebilecek alanları güncelliyoruz
                    existingTrainer.FirstName = trainer.FirstName;
                    existingTrainer.LastName = trainer.LastName;
                    existingTrainer.Phone = trainer.Phone;
                    existingTrainer.Email = trainer.Email;
                    existingTrainer.Specialization = trainer.Specialization;
                    existingTrainer.Bio = trainer.Bio;
                    existingTrainer.ExperienceYears = trainer.ExperienceYears;
                    existingTrainer.GymCenterId = trainer.GymCenterId;

                    // Eğer resim URL input'u varsa onu da güncelle (Opsiyonel)
                    existingTrainer.ProfileImageUrl = trainer.ProfileImageUrl;

                    // NOT: CreatedDate, HireDate, UserId ve IsActive alanlarına dokunmuyoruz.
                    // Böylece verinin tarihçesini ve kullanıcı bağını koruyoruz.

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

            // Hata durumunda dropdown'ı tekrar doldur
            ViewBag.GymCenters = new SelectList(
                await _context.GymCenters.Where(g => g.IsActive).ToListAsync(),
                "Id",
                "Name",
                trainer.GymCenterId
            );

            return View(trainer);
        }

        // GET: Trainer/Delete/5
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

        // POST: Trainer/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var trainer = await _context.Trainers.FindAsync(id);

            if (trainer != null)
            {
                // SOFT DELETE: Kaydı silmek yerine pasife çekiyoruz
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