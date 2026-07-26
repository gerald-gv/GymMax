
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GymMax.Models;
using GymMax.Data;
using GymMax.Enums;

[Authorize(Roles = "Administrador")]
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
        return View(await _context.Suscripciones
            .Include(s => s.Usuario)
            .Include(s => s.Plan)
            .ToListAsync());
    }

    // GET: SUSCRIPCIONS/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var suscripcion = await _context.Suscripciones
            .Include(s => s.Usuario)
            .Include(s => s.Plan)
            .FirstOrDefaultAsync(m => m.SuscripcionId == id);
        if (suscripcion == null)
        {
            return NotFound();
        }

        return View(suscripcion);
    }

    // GET: SUSCRIPCIONS/Create
    public async Task<IActionResult> Create()
    {
        // Select de clientes: solo usuarios con rol Cliente
        var clientes = await _context.Usuarios
            .Where(u => u.RolId == (int)RolUsuario.Cliente && u.Estado == GymMax.Enums.EstadoUsuario.Activo)
            .Select(u => new { Value = u.UsuarioId, Text = $"{u.Nombres} {u.Apellidos}" })
            .ToListAsync();
        ViewBag.UsuarioId = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(clientes, "Value", "Text");

        // Select de planes activos: formato "Nombre — S/ precio"
        var planes = await _context.Planes
            .Where(p => p.Activo)
            .Select(p => new { Value = p.PlanId, Text = $"{p.Nombre} — S/ {p.Precio}", Precio = p.Precio })
            .ToListAsync();
        ViewBag.PlanId = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(planes, "Value", "Text");

        // Precios de planes en JSON para autocompletar PrecioPactado con JavaScript
        ViewBag.PlanesJson = System.Text.Json.JsonSerializer.Serialize(
            planes.ToDictionary(p => p.Value, p => p.Precio));

        return View();
    }

    // POST: SUSCRIPCIONS/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("SuscripcionId,UsuarioId,PlanId,PrecioPactado,FechaInicio,FechaFin,Estado")] Suscripcion suscripcion)
    {
        ModelState.Remove("Usuario");
        ModelState.Remove("Plan");

        if (ModelState.IsValid)
        {
            _context.Add(suscripcion);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // Recargar selects si hay error de validación
        var clientes = await _context.Usuarios
            .Where(u => u.RolId == (int)RolUsuario.Cliente)
            .Select(u => new { Value = u.UsuarioId, Text = $"{u.Nombres} {u.Apellidos}" })
            .ToListAsync();
        ViewBag.UsuarioId = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(clientes, "Value", "Text", suscripcion.UsuarioId);

        var planes = await _context.Planes
            .Where(p => p.Activo)
            .Select(p => new { Value = p.PlanId, Text = $"{p.Nombre} — S/ {p.Precio}", Precio = p.Precio })
            .ToListAsync();
        ViewBag.PlanId = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(planes, "Value", "Text", suscripcion.PlanId);
        ViewBag.PlanesJson = System.Text.Json.JsonSerializer.Serialize(
            planes.ToDictionary(p => p.Value, p => p.Precio));

        return View(suscripcion);
    }

    // GET: SUSCRIPCIONS/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var suscripcion = await _context.Suscripciones.FindAsync(id);
        if (suscripcion == null) return NotFound();

        await CargarViewBagEdit(suscripcion.UsuarioId, suscripcion.PlanId);
        return View(suscripcion);
    }

    // POST: SUSCRIPCIONS/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("SuscripcionId,UsuarioId,PlanId,PrecioPactado,FechaInicio,FechaFin,Estado")] Suscripcion suscripcion)
    {
        if (id != suscripcion.SuscripcionId) return NotFound();

        ModelState.Remove("Usuario");
        ModelState.Remove("Plan");

        if (ModelState.IsValid)
        {
            try
            {
                var suscripcionDb = await _context.Suscripciones.FindAsync(id);
                if (suscripcionDb == null) return NotFound();

                suscripcionDb.UsuarioId     = suscripcion.UsuarioId;
                suscripcionDb.PlanId        = suscripcion.PlanId;
                suscripcionDb.PrecioPactado = suscripcion.PrecioPactado;
                suscripcionDb.FechaInicio   = suscripcion.FechaInicio;
                suscripcionDb.FechaFin      = suscripcion.FechaFin;
                suscripcionDb.Estado        = suscripcion.Estado;

                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!SuscripcionExists(suscripcion.SuscripcionId)) return NotFound();
                else throw;
            }
            return RedirectToAction(nameof(Index));
        }

        await CargarViewBagEdit(suscripcion.UsuarioId, suscripcion.PlanId);
        return View(suscripcion);
    }

    // Método auxiliar para no repetir la carga de selects en Edit GET y POST
    private async Task CargarViewBagEdit(int usuarioIdSeleccionado, int planIdSeleccionado)
    {
        var clientes = await _context.Usuarios
            .Where(u => u.RolId == (int)RolUsuario.Cliente)
            .Select(u => new { Value = u.UsuarioId, Text = $"{u.Nombres} {u.Apellidos}" })
            .ToListAsync();
        ViewBag.UsuarioId = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(clientes, "Value", "Text", usuarioIdSeleccionado);

        var planes = await _context.Planes
            .Where(p => p.Activo)
            .Select(p => new { Value = p.PlanId, Text = $"{p.Nombre} — S/ {p.Precio}", Precio = p.Precio })
            .ToListAsync();
        ViewBag.PlanId = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(planes, "Value", "Text", planIdSeleccionado);
        ViewBag.PlanesJson = System.Text.Json.JsonSerializer.Serialize(
            planes.ToDictionary(p => p.Value, p => p.Precio));
    }

    // GET: SUSCRIPCIONS/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var suscripcion = await _context.Suscripciones
            .Include(s => s.Usuario)
            .Include(s => s.Plan)
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
