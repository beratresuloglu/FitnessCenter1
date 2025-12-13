using Microsoft.AspNetCore.Mvc;

namespace FitnessCenterWebApplication.Controllers
{
    public class AccountController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
