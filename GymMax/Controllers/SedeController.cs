using Microsoft.AspNetCore.Mvc;

namespace GymMax.Controllers {
    public class SedeController : Controller {

        [HttpGet]
        public IActionResult Index() {
            return View();
        }

    }
}
