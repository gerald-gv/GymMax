using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GymMax.Controllers
{
    [Authorize(Roles = "Cliente")]
    public class ClienteController : Controller
    {
        // GET: /Cliente/Index
        public IActionResult Index()
        {
            return View();
        }
    }
}
