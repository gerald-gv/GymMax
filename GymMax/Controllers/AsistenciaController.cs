
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GymMax.Models;
using GymMax.Data;

public class AsistenciaController : Controller
{
    private readonly AppDbContext _context;

    public AsistenciaController(AppDbContext context)
    {
        _context = context;
    }

    // GET: ASISTENCIAS
    public async Task<IActionResult> Index()    
    {
        return View(await _context.Asistencias.ToListAsync());
    }

    // GET: ASISTENCIAS/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var asistencia = await _context.Asistencias
            .FirstOrDefaultAsync(m => m.AsistenciaId == id);
        if (asistencia == null)
        {
            return NotFound();
        }

        return View(asistencia);
    }

    // GET: ASISTENCIAS/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: ASISTENCIAS/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("AsistenciaId,UsuarioId,SedeId,FechaHoraEntrada,Usuario,Sede")] Asistencia asistencia)
    {
        if (ModelState.IsValid)
        {
            _context.Add(asistencia);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(asistencia);
    }

    // GET: ASISTENCIAS/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var asistencia = await _context.Asistencias.FindAsync(id);
        if (asistencia == null)
        {
            return NotFound();
        }
        return View(asistencia);
    }

    // POST: ASISTENCIAS/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("AsistenciaId,UsuarioId,SedeId,FechaHoraEntrada,Usuario,Sede")] Asistencia asistencia)
    {
        if (id != asistencia.AsistenciaId)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(asistencia);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!AsistenciaExists(asistencia.AsistenciaId))
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
        return View(asistencia);
    }

    // GET: ASISTENCIAS/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var asistencia = await _context.Asistencias
            .FirstOrDefaultAsync(m => m.AsistenciaId == id);
        if (asistencia == null)
        {
            return NotFound();
        }

        return View(asistencia);
    }

    // POST: ASISTENCIAS/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var asistencia = await _context.Asistencias.FindAsync(id);
        if (asistencia != null)
        {
            _context.Asistencias.Remove(asistencia);
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool AsistenciaExists(int id)
    {
        return _context.Asistencias.Any(e => e.AsistenciaId == id);
    }
}
