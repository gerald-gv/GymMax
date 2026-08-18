
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GymMax.Models;
using GymMax.Data;

[Authorize(Roles = "Administrador")]
public class PagoController : Controller
{
    private readonly AppDbContext _context;

    public PagoController(AppDbContext context)
    {
        _context = context;
    }

    // GET: PAGOS
    public async Task<IActionResult> Index()    
    {
        return View(await _context.Pagos
            .Include(p => p.Suscripcion).ThenInclude(s => s.Usuario)
            .Include(p => p.Suscripcion).ThenInclude(s => s.Plan)
            .ToListAsync());
    }

    // GET: PAGOS/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var pago = await _context.Pagos
            .Include(p => p.Suscripcion).ThenInclude(s => s.Usuario)
            .Include(p => p.Suscripcion).ThenInclude(s => s.Plan)
            .FirstOrDefaultAsync(m => m.PagoId == id);
        if (pago == null)
        {
            return NotFound();
        }

        return View(pago);
    }

    // GET: PAGOS/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: PAGOS/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("PagoId,SuscripcionId,Monto,FechaPago,Suscripcion")] Pago pago)
    {
        if (ModelState.IsValid)
        {
            _context.Add(pago);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(pago);
    }

    private bool PagoExists(int id)
    {
        return _context.Pagos.Any(e => e.PagoId == id);
    }
}
