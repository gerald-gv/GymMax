using GymMax.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GymMax.Controllers {
    public class SedesController : Controller {
        private readonly AppDbContext _context;

        public SedesController(AppDbContext context) {
            _context = context;
        }

        public async Task<IActionResult> Index() {
            var sedes = await _context.Sedes
                .Where(s => s.Activo)
                .ToListAsync();

            return View(sedes);
        }
    }
}
