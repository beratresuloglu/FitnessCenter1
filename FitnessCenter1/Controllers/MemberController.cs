using FitnessCenter1.Models;
using Microsoft.AspNetCore.Mvc;

namespace FitnessCenter1.Controllers
{
    public class MemberController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
        [HttpGet]
        public IActionResult AddMember()
        {
            return View();
        }
        [HttpPost]
        public IActionResult AddMember(Member member)
        {
            // Logic to add member would go here
            return RedirectToAction("Index");
        }
        public IActionResult EditMember()
        {
            return View();
        }

        public IActionResult DeleteMember()
        {
            return View();
        }


    }
}

