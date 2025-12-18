using FitnessCenterWebApplication.Data; // AppDbContext için gerekli
using FitnessCenterWebApplication.Models.Entities;
using FitnessCenterWebApplication.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace FitnessCenterWebApplication.Controllers
{
    public class AccountController : Controller
    {
        private readonly SignInManager<User> signInManager;
        private readonly UserManager<User> userManager;
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly AppDbContext _context; // 1. EKLENDİ: Veritabanı erişimi için

        // Constructor güncellendi: AppDbContext eklendi
        public AccountController(SignInManager<User> signInManager,
                                 UserManager<User> userManager,
                                 RoleManager<IdentityRole> roleManager,
                                 AppDbContext context)
        {
            this.signInManager = signInManager;
            this.userManager = userManager;
            this.roleManager = roleManager;
            this._context = context; // 1. ATAMA YAPILDI
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var result = await signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);

            if (result.Succeeded)
            {
                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError(string.Empty, "Invalid Login Attempt");
            return View(model);
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            // User oluşturma
            var user = new User
            {
                FirstName = model.Name,
                // Eğer ViewModel'de Soyad yoksa boş geçmemek için varsayılan atıyoruz
                LastName = "",
                NormalizedUserName = model.Email.ToUpper(),
                Email = model.Email,
                NormalizedEmail = model.Email.ToUpper(),
                UserName = model.Email
            };

            var result = await userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                // Rol Atama İşlemleri
                var roleExist = await roleManager.RoleExistsAsync("Member");
                if (!roleExist)
                {
                    var role = new IdentityRole("Member");
                    await roleManager.CreateAsync(role);
                }
                await userManager.AddToRoleAsync(user, "Member");

                // 2. EKLENDİ: Otomatik Member (Üye) Kaydı Oluşturma
                // Member tablosundaki zorunlu alanları (Required) doldurmamız şart.
                var newMember = new Member
                {
                    UserId = user.Id, // Identity User ile ilişkilendiriyoruz (Çok Önemli!)
                    FirstName = user.FirstName,
                    LastName = user.LastName ?? "Soyad Girilmedi", // Zorunlu alan hatası almamak için
                    Email = user.Email,
                    Phone = user.PhoneNumber ?? "5550000000", // Zorunlu alan. Kayıt formunda yoksa geçici değer.
                    JoinDate = DateTime.Now,
                    IsActive = true,
                    DateOfBirth = DateTime.Now.AddYears(-18), // Varsayılan tarih (Hata vermemesi için)
                    Gender = "Belirtilmedi"
                };

                _context.Members.Add(newMember);
                await _context.SaveChangesAsync();
                // ---------------------------------------------------------

                await signInManager.SignInAsync(user, isPersistent: false);
                return RedirectToAction("Login", "Account");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        // ... Diğer metodlar (VerifyEmail, ChangePassword) aynen kalacak ...
        [HttpGet]
        public IActionResult VerifyEmail()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyEmail(VerifyEmailViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await userManager.FindByNameAsync(model.Email);

            if (user == null)
            {
                ModelState.AddModelError("", "User not found!");
                return View(model);
            }
            else
            {
                return RedirectToAction("ChangePassword", "Account", new { username = user.UserName });
            }
        }

        [HttpGet]
        public IActionResult ChangePassword(string username)
        {
            if (string.IsNullOrEmpty(username))
            {
                return RedirectToAction("VerifyEmail", "Account");
            }

            return View(new ChangePasswordViewModel { Email = username });
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ModelState.AddModelError("", "Something went wrong");
                return View(model);
            }

            var user = await userManager.FindByNameAsync(model.Email);

            if (user == null)
            {
                ModelState.AddModelError("", "User not found!");
                return View(model);
            }

            var result = await userManager.RemovePasswordAsync(user);
            if (result.Succeeded)
            {
                result = await userManager.AddPasswordAsync(user, model.NewPassword);
                return RedirectToAction("Login", "Account");
            }
            else
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }

                return View(model);
            }
        }
    }
}