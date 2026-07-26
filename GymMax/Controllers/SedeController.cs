
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GymMax.Models;
using GymMax.Data;

public class SedeController : Controller
{
    private readonly AppDbContext _context;

    public SedeController(AppDbContext context)
    {
        _context = context;
    }

    // GET: SEDES
    public async Task<IActionResult> Index()    
    {
        return View(await _context.Sedes.ToListAsync());
    }

    // GET: SEDES/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var sede = await _context.Sedes
            .FirstOrDefaultAsync(m => m.SedeId == id);
        if (sede == null)
        {
            return NotFound();
        }

        return View(sede);
    }

    // GET: SEDES/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: SEDES/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("SedeId,Nombre,Direccion,Telefono,Horario,Activo")] Sede sede)
    {
        if (ModelState.IsValid)
        {
            _context.Add(sede);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(sede);
    }

    // GET: SEDES/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var sede = await _context.Sedes.FindAsync(id);
        if (sede == null)
        {
            return NotFound();
        }
        return View(sede);
    }

    // POST: SEDES/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("SedeId,Nombre,Direccion,Telefono,Horario,Activo")] Sede sede)
    {
        if (id != sede.SedeId)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(sede);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!SedeExists(sede.SedeId))
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
        return View(sede);
    }

    // GET: SEDES/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var sede = await _context.Sedes
            .FirstOrDefaultAsync(m => m.SedeId == id);
        if (sede == null)
        {
            return NotFound();
        }

        return View(sede);
    }

    // POST: SEDES/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var sede = await _context.Sedes.FindAsync(id);
        if (sede != null)
        {
            _context.Sedes.Remove(sede);
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool SedeExists(int id)
    {
        return _context.Sedes.Any(e => e.SedeId == id);
    }
}
