using Microsoft.AspNetCore.Mvc;

namespace GymMax.Controllers {
    public class AuthController : Controller {

        [HttpGet]
        public IActionResult Login() {
            return View();
        }
    }
}
