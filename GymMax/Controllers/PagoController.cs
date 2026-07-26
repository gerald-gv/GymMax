
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

    // GET: PAGOS/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var pago = await _context.Pagos.FindAsync(id);
        if (pago == null)
        {
            return NotFound();
        }
        return View(pago);
    }

    // POST: PAGOS/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("PagoId,SuscripcionId,Monto,FechaPago,Suscripcion")] Pago pago)
    {
        if (id != pago.PagoId)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(pago);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PagoExists(pago.PagoId))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return RedirectToAction(nameof(Index));
        }
        return View(pago);
    }

    // GET: PAGOS/Delete/5
    public async Task<IActionResult> Delete(int? id)
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

    // POST: PAGOS/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var pago = await _context.Pagos.FindAsync(id);
        if (pago != null)
        {
            _context.Pagos.Remove(pago);
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool PagoExists(int id)
    {
        return _context.Pagos.Any(e => e.PagoId == id);
    }
}
