
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
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create( [Bind("SuscripcionId,UsuarioId,PlanId,PrecioPactado,FechaInicio,FechaFin,Estado")] Suscripcion suscripcion) {
        ModelState.Remove("Usuario");
        ModelState.Remove("Plan");

        if (ModelState.IsValid) {
            await _suscripcionService.CrearAsync(suscripcion);
            return RedirectToAction(nameof(Index));
        }

        // Recargar selects si hay error de validación
        await CargarViewBags( suscripcion.UsuarioId, suscripcion.PlanId );
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
    public async Task<IActionResult> Edit( int id, [Bind("SuscripcionId,UsuarioId,PlanId,PrecioPactado,FechaInicio,FechaFin,Estado")] Suscripcion suscripcion) {
        if (id != suscripcion.SuscripcionId) {
            return NotFound();
        }

        ModelState.Remove("Usuario");
        ModelState.Remove("Plan");

        if (ModelState.IsValid) {
            var actualizado = await _suscripcionService.ActualizarAsync(suscripcion);

            if (!actualizado) {
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
        await _suscripcionService.EliminarAsync(id);

        return RedirectToAction(nameof(Index));
    }

    // Metodo auxiliar para cargar los datos necesarios para los selects
    private async Task CargarViewBags(
        int? usuarioIdSeleccionado = null,
        int? planIdSeleccionado = null) {
        // Select de clientes: solo usuarios con rol Cliente
        var clientes = await _suscripcionService.ObtenerClientesActivosAsync();

        ViewBag.UsuarioId = new SelectList(
            clientes.Select(u => new {
                Value = u.UsuarioId,
                Text = $"{u.Nombres} {u.Apellidos}"
            }),
            "Value",
            "Text",
            usuarioIdSeleccionado
        );

        // Select de planes activos: formato "Nombre — S/ precio"
        var planes = await _suscripcionService.ObtenerPlanesActivosAsync();

        ViewBag.PlanId = new SelectList(
            planes.Select(p => new {
                Value = p.PlanId,
                Text = $"{p.Nombre} — S/ {p.Precio}",
                Precio = p.Precio
            }),
            "Value",
            "Text",
            planIdSeleccionado
        );

        // Precios de planes en JSON para autocompletar PrecioPactado
        ViewBag.PlanesJson = JsonSerializer.Serialize(planes.ToDictionary(p => p.PlanId, p => p.Precio )
        );
    }
}
