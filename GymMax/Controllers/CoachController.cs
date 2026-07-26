
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GymMax.Models;
using GymMax.Data;

[Authorize(Roles = "Administrador")]
public class CoachController : Controller
{
    private readonly AppDbContext _context;

    public CoachController(AppDbContext context)
    {
        _context = context;
    }

    // GET: COACHS
    public async Task<IActionResult> Index()    
    {
        return View(await _context.Coaches
            .Include(c => c.Usuario)
            .Include(c => c.Sede)
            .ToListAsync());
    }

    // GET: COACHS/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var coach = await _context.Coaches
            .Include(c => c.Usuario)
            .Include(c => c.Sede)
            .FirstOrDefaultAsync(m => m.CoachId == id);
        if (coach == null)
        {
            return NotFound();
        }

        return View(coach);
    }

    // GET: COACHS/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: COACHS/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("CoachId,UsuarioId,SedeId,FechaIngreso,Activo,Usuario,Sede")] Coach coach)
    {
        if (ModelState.IsValid)
        {
            _context.Add(coach);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(coach);
    }

    // GET: COACHS/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var coach = await _context.Coaches
            .Include(c => c.Usuario)
            .FirstOrDefaultAsync(m => m.CoachId == id);
        if (coach == null)
        {
            return NotFound();
        }

        // Cargar sedes para el select — formato "ID — Nombre" para mayor guía
        ViewBag.SedeId = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(
            _context.Sedes.Select(s => new { Value = s.SedeId, Text = $"{s.SedeId} — {s.Nombre}" }),
            "Value", "Text", coach.SedeId);

        return View(coach);
    }

    // POST: COACHS/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("CoachId,SedeId,FechaIngreso,Activo")] Coach coach)
    {
        if (id != coach.CoachId)
        {
            return NotFound();
        }

        // Ignoramos validaciones de propiedades que no se editan aquí
        ModelState.Remove("UsuarioId");
        ModelState.Remove("Usuario");

        if (ModelState.IsValid)
        {
            try
            {
                var coachDb = await _context.Coaches.FindAsync(id);
                if (coachDb == null)
                {
                    return NotFound();
                }

                coachDb.SedeId       = coach.SedeId;
                coachDb.FechaIngreso = coach.FechaIngreso;
                coachDb.Activo       = coach.Activo;

                // Sincronizar EstadoUsuario con el nuevo Activo del coach
                var usuarioAsociado = await _context.Usuarios.FindAsync(coachDb.UsuarioId);
                if (usuarioAsociado != null)
                    usuarioAsociado.Estado = coach.Activo
                        ? GymMax.Enums.EstadoUsuario.Activo
                        : GymMax.Enums.EstadoUsuario.Inactivo;

                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CoachExists(coach.CoachId))
                    return NotFound();
                else
                    throw;
            }
            return RedirectToAction(nameof(Index));
        }

        ViewBag.SedeId = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(
            _context.Sedes.Select(s => new { Value = s.SedeId, Text = $"{s.SedeId} — {s.Nombre}" }),
            "Value", "Text", coach.SedeId);

        return View(coach);
    }

    // GET: COACHS/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var coach = await _context.Coaches
            .Include(c => c.Usuario)
            .Include(c => c.Sede)
            .FirstOrDefaultAsync(m => m.CoachId == id);
        if (coach == null)
        {
            return NotFound();
        }

        return View(coach);
    }

    // POST: COACHS/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var coach = await _context.Coaches.FindAsync(id);
        if (coach != null)
        {
            _context.Coaches.Remove(coach);
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool CoachExists(int id)
    {
        return _context.Coaches.Any(e => e.CoachId == id);
    }
}
