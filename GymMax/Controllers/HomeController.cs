using GymMax.Data;
using GymMax.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GymMax.Controllers {
    public class HomeController : Controller {

        private readonly AppDbContext _context;

        public HomeController(AppDbContext context) {
            _context = context;
        }

        public async Task<IActionResult> Index() {
            var vm = new HomeViewModel {
                Planes = await _context.Planes
                    .Where(p => p.Activo)
                    .OrderBy(p => p.PlanId)
                    .Take(3)
                    .ToListAsync()
            };

            return View(vm);
        }
    }
}
