
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GymMax.Models;
using GymMax.Data;

public class SuscripcionController : Controller
{
    private readonly AppDbContext _context;

    public SuscripcionController(AppDbContext context)
    {
        _context = context;
    }

    // GET: SUSCRIPCIONS
    public async Task<IActionResult> Index()    
    {
        return View(await _context.Suscripciones.ToListAsync());
    }

    // GET: SUSCRIPCIONS/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var suscripcion = await _context.Suscripciones
            .FirstOrDefaultAsync(m => m.SuscripcionId == id);
        if (suscripcion == null)
        {
            return NotFound();
        }

        return View(suscripcion);
    }

    // GET: SUSCRIPCIONS/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: SUSCRIPCIONS/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("SuscripcionId,UsuarioId,PlanId,PrecioPactado,FechaInicio,FechaFin,Estado,Usuario,Plan")] Suscripcion suscripcion)
    {
        if (ModelState.IsValid)
        {
            _context.Add(suscripcion);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(suscripcion);
    }

    // GET: SUSCRIPCIONS/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var suscripcion = await _context.Suscripciones.FindAsync(id);
        if (suscripcion == null)
        {
            return NotFound();
        }
        return View(suscripcion);
    }

    // POST: SUSCRIPCIONS/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("SuscripcionId,UsuarioId,PlanId,PrecioPactado,FechaInicio,FechaFin,Estado,Usuario,Plan")] Suscripcion suscripcion)
    {
        if (id != suscripcion.SuscripcionId)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(suscripcion);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!SuscripcionExists(suscripcion.SuscripcionId))
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
        return View(suscripcion);
    }

    // GET: SUSCRIPCIONS/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var suscripcion = await _context.Suscripciones
            .FirstOrDefaultAsync(m => m.SuscripcionId == id);
        if (suscripcion == null)
        {
            return NotFound();
        }

        return View(suscripcion);
    }

    // POST: SUSCRIPCIONS/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var suscripcion = await _context.Suscripciones.FindAsync(id);
        if (suscripcion != null)
        {
            _context.Suscripciones.Remove(suscripcion);
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool SuscripcionExists(int id)
    {
        return _context.Suscripciones.Any(e => e.SuscripcionId == id);
    }
}
