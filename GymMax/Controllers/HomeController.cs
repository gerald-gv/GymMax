using Microsoft.AspNetCore.Mvc;

namespace GymMax.Controllers {
    public class HomeController : Controller {
        public IActionResult Index() {
            return View();
        }


    }
}

