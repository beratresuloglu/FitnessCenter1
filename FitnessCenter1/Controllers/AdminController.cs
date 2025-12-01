using Microsoft.AspNetCore.Mvc;

namespace FitnessCenter1.Controllers
{
    public class AdminController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
        
    }
}
