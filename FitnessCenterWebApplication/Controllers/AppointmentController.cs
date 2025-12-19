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
                    .ThenInclude(m => m.User) // <-- BU SATIRI EKLEMELİSİN (Identity bilgilerini çeker)
                .AsQueryable();

            // Filtreleme mantığı aynı kalacak
            if (!User.IsInRole("Admin"))
            {
                var user = await _userManager.GetUserAsync(User);
                var member = await _context.Members.FirstOrDefaultAsync(m => m.UserId == user.Id);

                if (member != null)
                {
                    query = query.Where(a => a.MemberId == member.Id);
                }
                else
                {
                    return View(new List<Appointment>());
                }
            }

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
            // Validasyon temizliği
            ModelState.Remove("Member");
            ModelState.Remove("Trainer");
            ModelState.Remove("Service");
            ModelState.Remove("ApprovedBy");
            ModelState.Remove("ApprovedDate");

            // KULLANICI ARTIK ENDTIME GİRMİYOR, BİZ HESAPLAYACAĞIZ
            ModelState.Remove("EndTime");

            if (appointment.TrainerId <= 0) ModelState.AddModelError("TrainerId", "Antrenör seçilmedi.");

            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);
                var member = await _context.Members.FirstOrDefaultAsync(m => m.UserId == user.Id);
                if (member == null) return RedirectToAction("Create"); // Veya hata sayfasına

                appointment.MemberId = member.Id;

                // --- KRİTİK NOKTA: Bitiş Saatini Otomatik Hesapla ---
                var service = await _context.Services.FindAsync(appointment.ServiceId);
                if (service != null)
                {
                    appointment.TotalPrice = service.Price;

                    // Başlangıç saatine hizmet süresini ekle = Bitiş Saati
                    appointment.EndTime = appointment.StartTime.Add(TimeSpan.FromMinutes(service.DurationMinutes));
                }
                // ----------------------------------------------------

                // Çakışma kontrolü (Backend tarafında son bir güvenlik önlemi olarak kalsın)
                if (!await IsTrainerAvailable(appointment.TrainerId, appointment.AppointmentDate, appointment.StartTime, appointment.EndTime))
                {
                    ModelState.AddModelError("", "Bu saat az önce doldu, lütfen başka saat seçiniz.");
                    return ReloadView(appointment);
                }

                appointment.CreatedDate = DateTime.Now;
                appointment.Status = AppointmentStatus.Pending;

                _context.Add(appointment);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Randevu başarıyla oluşturuldu.";
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
                // GÜNCELLEME: Karakter Sınırı Kontrolü (Backend)
                if (!string.IsNullOrEmpty(reason) && reason.Length > 100)
                {
                    // 100 karakterden fazlasını kes
                    reason = reason.Substring(0, 100);
                }

                appointment.Status = AppointmentStatus.Cancelled;
                appointment.CancellationReason = reason;
                appointment.UpdatedDate = DateTime.Now;

                _context.Update(appointment);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Randevu iptal edildi.";
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

        [HttpGet]
        public async Task<JsonResult> GetTrainersByService(int serviceId)
        {
            // TrainerService tablosunu kullanarak o hizmeti veren hocaları buluyoruz
            var trainers = await _context.Trainers
                .Where(t => t.IsActive && t.TrainerServices.Any(ts => ts.ServiceId == serviceId))
                .Select(t => new
                {
                    id = t.Id,
                    fullName = t.FullName
                })
                .ToListAsync();

            return Json(trainers);
        }

        // 2. Eğitmen ve Tarihe Göre Dolu Saatleri Getir (JSON)
        [HttpGet]
        public async Task<JsonResult> GetBookedHours(int trainerId, DateTime date)
        {
            // O hocanın, o tarihteki iptal edilmemiş randevularını çekiyoruz
            var appointments = await _context.Appointments
                .Where(a => a.TrainerId == trainerId
                            && a.AppointmentDate.Date == date.Date
                            && a.Status != AppointmentStatus.Cancelled)
                .Select(a => new
                {
                    start = a.StartTime.ToString(@"hh\:mm"),
                    end = a.EndTime.ToString(@"hh\:mm")
                })
                .ToListAsync();

            return Json(appointments);
        }

        [HttpGet]
        public async Task<JsonResult> GetAvailableSlots(int trainerId, int serviceId, DateTime date)
        {
            // 1. Hizmet Süresi
            var service = await _context.Services.FindAsync(serviceId);
            if (service == null) return Json(new List<object>());
            int duration = service.DurationMinutes;

            // 2. Eğitmenin O GÜN (DayOfWeek) için tanımlı çalışma saatlerini çek
            // Örn: Pazartesi günü hem 09:00-12:00 hem de 14:00-18:00 çalışıyor olabilir.
            var availabilities = await _context.TrainerAvailabilities
                .Where(ta => ta.TrainerId == trainerId && ta.DayOfWeek == date.DayOfWeek && ta.IsActive)
                .OrderBy(ta => ta.StartTime)
                .ToListAsync();

            // Eğer o gün hiç kayıt yoksa eğitmen çalışmıyor demektir.
            if (!availabilities.Any())
            {
                return Json(new List<object>()); // Boş liste döner
            }

            // 3. O günkü dolu randevuları çek
            var bookedAppointments = await _context.Appointments
                .Where(a => a.TrainerId == trainerId
                            && a.AppointmentDate.Date == date.Date
                            && a.Status != AppointmentStatus.Cancelled)
                .ToListAsync();

            var slots = new List<object>();

            // 4. Her bir çalışma aralığı için slotları oluştur
            foreach (var shift in availabilities)
            {
                TimeSpan currentSlot = shift.StartTime;
                TimeSpan shiftEnd = shift.EndTime;

                // Vardiya bitimine kadar döngü
                while (currentSlot.Add(TimeSpan.FromMinutes(duration)) <= shiftEnd)
                {
                    var slotEnd = currentSlot.Add(TimeSpan.FromMinutes(duration));

                    // Çakışma Kontrolü (Randevularla)
                    bool isBooked = bookedAppointments.Any(a =>
                        (currentSlot >= a.StartTime && currentSlot < a.EndTime) ||
                        (slotEnd > a.StartTime && slotEnd <= a.EndTime) ||
                        (currentSlot <= a.StartTime && slotEnd >= a.EndTime)
                    );

                    slots.Add(new
                    {
                        time = currentSlot.ToString(@"hh\:mm"),
                        isFull = isBooked
                    });

                    // Bir sonraki slota geç
                    currentSlot = currentSlot.Add(TimeSpan.FromMinutes(duration));
                }
            }

            return Json(slots);
        }
    }
}