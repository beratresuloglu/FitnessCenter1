using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Identity;
using FitnessCenterWebApplication.Data;
using FitnessCenterWebApplication.Models.Entities;
using System.Security.Claims;

namespace FitnessCenterWebApplication.Controllers
{
    [Authorize] // Sadece giriş yapmış kullanıcılar erişebilir
    public class AppointmentController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<User> _userManager; // Giriş yapan kullanıcıyı bulmak için

        public AppointmentController(AppDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Appointment/Index
        public async Task<IActionResult> Index()
        {
            var query = _context.Appointments
                .Include(a => a.Service)
                .Include(a => a.Trainer)
                .Include(a => a.Member)
                .AsQueryable();

            // Eğer Admin değilse, sadece kendi randevularını görsün
            if (!User.IsInRole("Admin"))
            {
                var user = await _userManager.GetUserAsync(User);
                // Not: Member tablosu ile User tablosu arasında ilişki olduğunu varsayıyoruz.
                // Eğer MemberId User tablosunda değilse, Member tablosundan Email ile eşleştirme yapılmalı.
                var member = await _context.Members.FirstOrDefaultAsync(m => m.UserId == user.Id);

                if (member != null)
                {
                    query = query.Where(a => a.MemberId == member.Id);
                }
                else
                {
                    // Üye kaydı yoksa boş liste dönebilir veya hata sayfasına yönlendirebiliriz
                    return View(new List<Appointment>());
                }
            }

            // Tarihe göre sırala (En yeni en üstte)
            var appointments = await query.OrderByDescending(a => a.AppointmentDate).ToListAsync();
            return View(appointments);
        }

        // GET: Appointment/Create
        public async Task<IActionResult> Create(int? trainerId, int? serviceId)
        {
            // Dropdownları doldur
            ViewData["Services"] = new SelectList(_context.Services.Where(s => s.IsActive), "Id", "Name", serviceId);
            ViewData["Trainers"] = new SelectList(_context.Trainers.Where(t => t.IsActive), "Id", "FullName", trainerId);

            return View();
        }

        // POST: Appointment/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Appointment appointment)
        {
            // 1. Navigation Propertyleri Validasyondan Çıkar
            ModelState.Remove("Member");
            ModelState.Remove("Trainer");
            ModelState.Remove("Service");
            ModelState.Remove("ApprovedBy"); // Admin onaylayacak
            ModelState.Remove("ApprovedDate");

            if (ModelState.IsValid)
            {
                // 2. Giriş Yapan Üyeyi Bul ve Ata
                var user = await _userManager.GetUserAsync(User);
                var member = await _context.Members.FirstOrDefaultAsync(m => m.UserId == user.Id);

                if (member == null)
                {
                    ModelState.AddModelError("", "Üye kaydınız bulunamadı. Lütfen profilinizi tamamlayın.");
                    return ReloadView(appointment);
                }
                appointment.MemberId = member.Id;

                // 3. Hizmet Fiyatını ve Süresini Çek
                var service = await _context.Services.FindAsync(appointment.ServiceId);
                if (service == null) return NotFound();

                appointment.TotalPrice = service.Price;
                // Bitiş saatini hizmet süresine göre otomatik ayarlayabiliriz veya kullanıcı seçimi kalabilir.
                // Biz kullanıcı seçimine sadık kalalım ama süreyi kontrol edelim (Opsiyonel)

                if (!await IsTrainerAvailable(appointment.TrainerId, appointment.AppointmentDate, appointment.StartTime, appointment.EndTime))
                {
                    ModelState.AddModelError("", "Seçilen saatlerde antrenör müsait değil. Lütfen başka bir saat seçiniz.");
                    return ReloadView(appointment);
                }

                // 5. Varsayılan Değerler
                appointment.CreatedDate = DateTime.Now;
                appointment.Status = AppointmentStatus.Pending; // Onay Bekliyor
                appointment.IsApproved = false;

                _context.Add(appointment);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Randevu talebiniz başarıyla oluşturuldu. Onay bekleniyor.";
                return RedirectToAction(nameof(Index));
            }

            return ReloadView(appointment);
        }

        // -----------------------------------------------------------
        // 1. İPTAL (CANCEL) İŞLEMLERİ
        // -----------------------------------------------------------

        // GET: Appointment/Cancel/5 (İptal Sayfasını Getir)
        public async Task<IActionResult> Cancel(int? id)
        {
            if (id == null) return NotFound();

            var appointment = await _context.Appointments
                .Include(a => a.Service)
                .Include(a => a.Trainer)
                .Include(a => a.Member)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (appointment == null) return NotFound();

            return View(appointment);
        }

        // POST: Appointment/Cancel/5 (İptal İşlemini Yap)
        [HttpPost, ActionName("Cancel")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelConfirmed(int id, string reason)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment != null)
            {
                appointment.Status = AppointmentStatus.Cancelled;
                appointment.CancellationReason = reason;
                appointment.UpdatedDate = DateTime.Now; // İptal zamanı

                _context.Update(appointment);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Randevu başarıyla iptal edildi.";
            }
            return RedirectToAction(nameof(Index));
        }

        // -----------------------------------------------------------
        // 2. ONAY (APPROVE) İŞLEMLERİ
        // -----------------------------------------------------------

        // GET: Appointment/Approve/5 (Onay Sayfasını Getir)
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Approve(int? id)
        {
            if (id == null) return NotFound();

            var appointment = await _context.Appointments
                .Include(a => a.Service)
                .Include(a => a.Trainer)
                .Include(a => a.Member) // Üye bilgisini görmek önemli
                .FirstOrDefaultAsync(m => m.Id == id);

            if (appointment == null) return NotFound();

            return View(appointment);
        }

        // POST: Appointment/Approve/5 (Onay İşlemini Yap)
        [HttpPost, ActionName("Approve")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ApproveConfirmed(int id)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment != null)
            {
                appointment.IsApproved = true;
                appointment.Status = AppointmentStatus.Approved;
                appointment.ApprovedDate = DateTime.Now;
                appointment.ApprovedBy = User.Identity?.Name;

                _context.Update(appointment);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Randevu onaylandı.";
            }
            return RedirectToAction(nameof(Index));
        }

        //// DELETE: Appointment/Delete/5 (Sadece Admin - Tamamen silmek isterse)
        //[Authorize(Roles = "Admin")]
        //public async Task<IActionResult> Delete(int? id)
        //{
        //    if (id == null) return NotFound();

        //    var appointment = await _context.Appointments
        //        .Include(a => a.Member)
        //        .Include(a => a.Trainer)
        //        .Include(a => a.Service)
        //        .FirstOrDefaultAsync(m => m.Id == id);

        //    if (appointment == null) return NotFound();

        //    return View(appointment);
        //}

        //[HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        //[Authorize(Roles = "Admin")]
        //public async Task<IActionResult> DeleteConfirmed(int id)
        //{
        //    var appointment = await _context.Appointments.FindAsync(id);
        //    if (appointment != null)
        //    {
        //        _context.Appointments.Remove(appointment);
        //        await _context.SaveChangesAsync();
        //        TempData["Success"] = "Randevu kaydı tamamen silindi.";
        //    }
        //    return RedirectToAction(nameof(Index));
        //}

        // Yardımcı Metot: View'ı tekrar doldurma (DRY prensibi)
        private IActionResult ReloadView(Appointment appointment)
        {
            ViewData["Services"] = new SelectList(_context.Services.Where(s => s.IsActive), "Id", "Name", appointment.ServiceId);
            ViewData["Trainers"] = new SelectList(_context.Trainers.Where(t => t.IsActive), "Id", "FullName", appointment.TrainerId);
            return View(appointment);
        }

        // Yardımcı Metot: Çakışma Kontrolü
        private async Task<bool> IsTrainerAvailable(int trainerId, DateTime date, TimeSpan start, TimeSpan end)
        {
            // Veritabanında aynı antrenörün, aynı tarihteki randevularını getir
            var conflictingAppointment = await _context.Appointments
                .Where(a => a.TrainerId == trainerId
                            && a.AppointmentDate.Date == date.Date
                            && a.Status != AppointmentStatus.Cancelled) // İptal edilenler çakışma yaratmaz
                .AnyAsync(a =>
                    (start >= a.StartTime && start < a.EndTime) || // Yeni başlama saati, mevcut aralığın içindeyse
                    (end > a.StartTime && end <= a.EndTime) ||     // Yeni bitiş saati, mevcut aralığın içindeyse
                    (start <= a.StartTime && end >= a.EndTime)     // Yeni randevu, mevcutu kapsıyorsa
                );

            return !conflictingAppointment;
        }
    }
}