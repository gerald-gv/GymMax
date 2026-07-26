using Microsoft.AspNetCore.Mvc;

namespace GymMax.Controllers {
    public class HomeController : Controller {

        [HttpGet]
        public IActionResult Index() {
            return View();
        }
    }
}
