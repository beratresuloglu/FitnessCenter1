using FitnessCenter1.Models;
using Microsoft.AspNetCore.Mvc;

namespace FitnessCenter1.Controllers
{
    public class AccountController : Controller
    {
        List<Member> members = new List<Member>()
        {
            new Member() { MemberID=1,MemberName="Ali",MemberSurname="Veli",MemberEmail="aliveli@gmail.com",MemberUserName="aliveli",MemberPassword="12345"}
        };

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(Member m)
        {
            // 1. Formdan gelen veri (m) ile listedeki üyeyi eşleştiriyoruz.
            // FirstOrDefault: Eşleşen ilk kaydı getirir, yoksa null döner.
            var user = members.FirstOrDefault(x => x.MemberUserName == m.MemberUserName && x.MemberPassword == m.MemberPassword);

            if (user != null)
            {
                // --- GİRİŞ BAŞARILI ---

                // 2. Session Oluşturma (Sunucuya not düşüyoruz)
                // Kullanıcının e-postasını veya ID'sini saklayabilirsin.
                HttpContext.Session.SetString("MemberUserName", user.MemberUserName);
                HttpContext.Session.SetInt32("MemberID", user.MemberID);

                // 3. Yönlendirme
                // Giriş başarılı olduğu için Ana Sayfaya (Home/Index) gönderiyoruz.
                return RedirectToAction("Index", "Home");
            }
            else
            {
                // --- GİRİŞ HATALI ---

                // Kullanıcıya hata mesajı göstermek için ViewBag kullanabilirsin.
                ViewBag.ErrorMessage = "Kullanıcı adı veya şifre hatalı!";
                return View();
            }
        }

        public IActionResult Logout()
        {
            // Çıkış yapıldığında Session'ı temizliyoruz.
            HttpContext.Session.Clear();

            // Giriş sayfasına geri gönderiyoruz.
            return RedirectToAction("Login");
        }


    }
}
