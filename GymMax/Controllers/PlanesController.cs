using GymMax.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GymMax.Controllers {
    public class PlanesController : Controller {
        private readonly AppDbContext _context;

        public PlanesController(AppDbContext context) {
            _context = context;
        }

        public async Task<IActionResult> Index() {
            var planes = await _context.Planes
                .Where(p => p.Activo)
                .ToListAsync();

            return View(planes);
        }
    }
}
