
using GymMax.Data;
using GymMax.Enums;
using GymMax.Models;
using GymMax.Services.Suscripciones;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

[Authorize(Roles = "Administrador")]
public class SuscripcionController : Controller
{
    private readonly ISuscripcionService _suscripcionService;

    public SuscripcionController(ISuscripcionService suscripcionService) {
        _suscripcionService = suscripcionService;
    }

    // GET: SUSCRIPCIONS
    public async Task<IActionResult> Index() {
        var suscripciones = await _suscripcionService.ObtenerTodasAsync();

        return View(suscripciones);
    }

    // GET: SUSCRIPCIONS/Details/5
    public async Task<IActionResult> Details(int? id) {
        if (id == null) {
            return NotFound();
        }

        var suscripcion = await _suscripcionService.ObtenerPorIdAsync(id.Value);

        if (suscripcion == null) {
            return NotFound();
        }

        return View(suscripcion);
    }

    // GET: SUSCRIPCIONS/Create
    public async Task<IActionResult> Create() {
        await CargarViewBags();
        return View();
    }

    // POST: SUSCRIPCIONS/Create
    // POST: SUSCRIPCIONS/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
    [Bind("UsuarioId,PlanId,PrecioPactado,FechaFin,Estado")] Suscripcion suscripcion)
    {

        ModelState.Remove("Usuario");
        ModelState.Remove("Plan");
        ModelState.Remove("FechaInicio");

        suscripcion.FechaInicio = DateOnly.FromDateTime(DateTime.Now);

        if (suscripcion.FechaFin <= suscripcion.FechaInicio)
        {
            ModelState.AddModelError(nameof(suscripcion.FechaFin),
                "La fecha de fin debe ser posterior a la fecha de inicio.");
        }

        if (ModelState.IsValid)
        {
            await _suscripcionService.CrearAsync(suscripcion);
            return RedirectToAction(nameof(Index));
        }

        await CargarViewBags(suscripcion.UsuarioId, suscripcion.PlanId);
        return View(suscripcion);
    }

    // GET: SUSCRIPCIONS/Edit/5
    public async Task<IActionResult> Edit(int? id) {
        if (id == null) {
            return NotFound();
        }

        var suscripcion = await _suscripcionService.ObtenerPorIdAsync(id.Value);

        if (suscripcion == null) {
            return NotFound();
        }

        await CargarViewBags( suscripcion.UsuarioId, suscripcion.PlanId );
        return View(suscripcion);
    }

    // POST: SUSCRIPCIONS/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("SuscripcionId,UsuarioId,PlanId,PrecioPactado,FechaInicio,FechaFin,Estado")] Suscripcion suscripcion)
    {
        if (id != suscripcion.SuscripcionId)
        {
            return NotFound();
        }
        ModelState.Remove("Usuario");
        ModelState.Remove("Plan");
        if (suscripcion.FechaFin <= suscripcion.FechaInicio)
        {
            ModelState.AddModelError(nameof(suscripcion.FechaFin),
                "La fecha de fin debe ser posterior a la fecha de inicio.");
        }

        if (ModelState.IsValid) {
            var actualizado = await _suscripcionService.ActualizarAsync(suscripcion);

            if (!actualizado) {
                // Puede ser que no exista o que sea una cancelada que no se puede reactivar
                var original = await _suscripcionService.ObtenerPorIdAsync(suscripcion.SuscripcionId);
                if (original?.Estado == EstadoSuscripcion.Cancelada && suscripcion.Estado == EstadoSuscripcion.Activa) {
                    ModelState.AddModelError(nameof(suscripcion.Estado),
                        "Una suscripción cancelada no puede reactivarse. Crea una nueva suscripción si el cliente desea retomar el plan.");
                    await CargarViewBags(suscripcion.UsuarioId, suscripcion.PlanId);
                    return View(suscripcion);
                }
                return NotFound();
            }

            return RedirectToAction(nameof(Index));
        }

        await CargarViewBags( suscripcion.UsuarioId, suscripcion.PlanId );
        return View(suscripcion);
    }

    // GET: SUSCRIPCIONS/Delete/5
    public async Task<IActionResult> Delete(int? id) {
        if (id == null) {
            return NotFound();
        }

        var suscripcion = await _suscripcionService.ObtenerPorIdAsync(id.Value);

        if (suscripcion == null) {
            return NotFound();
        }

        return View(suscripcion);
    }

    // POST: SUSCRIPCIONS/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id) {
        await _suscripcionService.CancelarAsync(id);
        return RedirectToAction(nameof(Index));
    }

    // Metodo auxiliar para cargar los datos necesarios para los selects
    private async Task CargarViewBags(
    int? usuarioIdSeleccionado = null,
    int? planIdSeleccionado = null)
    {
        var clientes = await _suscripcionService.ObtenerClientesActivosAsync();
        ViewBag.UsuarioId = new SelectList(
            clientes.Select(u => new { Value = u.UsuarioId, Text = $"{u.Nombres} {u.Apellidos}" }),
            "Value", "Text", usuarioIdSeleccionado
        );

        var planes = await _suscripcionService.ObtenerPlanesActivosAsync();
        ViewBag.PlanId = new SelectList(
            planes.Select(p => new { Value = p.PlanId, Text = $"{p.Nombre} — S/ {p.Precio}" }),
            "Value", "Text", planIdSeleccionado
        );

        // Ahora incluye precio Y duracionDias
        ViewBag.PlanesJson = JsonSerializer.Serialize(
            planes.ToDictionary(
                p => p.PlanId,
                p => new { precio = p.Precio, duracionDias = p.DuracionDias }
            )
        );
    }
}
