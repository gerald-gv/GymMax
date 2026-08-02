
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GymMax.Models;
using GymMax.Data;

[Authorize(Roles = "Administrador")]
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
        return View(await _context.Asistencias
            .Include(a => a.Usuario)
            .Include(a => a.Sede)
            .ToListAsync());
    }

    // GET: ASISTENCIAS/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var asistencia = await _context.Asistencias
            .Include(a => a.Usuario)
            .Include(a => a.Sede)
            .FirstOrDefaultAsync(m => m.AsistenciaId == id);
        if (asistencia == null)
        {
            return NotFound();
        }

        return View(asistencia);
    }

    // GET: ASISTENCIAS/Create
    public async Task<IActionResult> Create()
    {
        await CargarViewBagAsistencia(null, null);
        return View();
    }

    // POST: ASISTENCIAS/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("AsistenciaId,UsuarioId,SedeId,FechaHoraEntrada")] Asistencia asistencia)
    {
        ModelState.Remove("Usuario");
        ModelState.Remove("Sede");

        if (ModelState.IsValid)
        {
            _context.Add(asistencia);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        await CargarViewBagAsistencia(asistencia.UsuarioId, asistencia.SedeId);
        return View(asistencia);
    }

    // GET: ASISTENCIAS/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var asistencia = await _context.Asistencias.FindAsync(id);
        if (asistencia == null) return NotFound();

        await CargarViewBagAsistencia(asistencia.UsuarioId, asistencia.SedeId);
        return View(asistencia);
    }

    // POST: ASISTENCIAS/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("AsistenciaId,UsuarioId,SedeId,FechaHoraEntrada")] Asistencia asistencia)
    {
        if (id != asistencia.AsistenciaId) return NotFound();

        ModelState.Remove("Usuario");
        ModelState.Remove("Sede");

        if (ModelState.IsValid)
        {
            try
            {
                var asistenciaDb = await _context.Asistencias.FindAsync(id);
                if (asistenciaDb == null) return NotFound();

                asistenciaDb.UsuarioId       = asistencia.UsuarioId;
                asistenciaDb.SedeId          = asistencia.SedeId;
                asistenciaDb.FechaHoraEntrada = asistencia.FechaHoraEntrada;

                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!AsistenciaExists(asistencia.AsistenciaId)) return NotFound();
                else throw;
            }
            return RedirectToAction(nameof(Index));
        }

        await CargarViewBagAsistencia(asistencia.UsuarioId, asistencia.SedeId);
        return View(asistencia);
    }

    // Método auxiliar para cargar selects de Usuario y Sede
    private async Task CargarViewBagAsistencia(int? usuarioId, int? sedeId)
    {
        var usuarios = await _context.Usuarios
            .Where(u => u.RolId == (int)GymMax.Enums.RolUsuario.Cliente && u.Estado == GymMax.Enums.EstadoUsuario.Activo)
            .Select(u => new { Value = u.UsuarioId, Text = $"{u.Nombres} {u.Apellidos}" })
            .ToListAsync();
        ViewBag.UsuarioId = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(usuarios, "Value", "Text", usuarioId);

        var sedes = await _context.Sedes
            .Where(s => s.Activo)
            .Select(s => new { Value = s.SedeId, Text = s.Nombre })
            .ToListAsync();
        ViewBag.SedeId = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(sedes, "Value", "Text", sedeId);
    }

    // GET: ASISTENCIAS/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var asistencia = await _context.Asistencias
            .Include(a => a.Usuario)
            .Include(a => a.Sede)
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
