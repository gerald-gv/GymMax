using GymMax.Data;
using GymMax.Models;
using GymMax.Services.Sedes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[Authorize(Roles = "Administrador")]
public class SedeController : Controller
{
    private readonly ISedeService _sedeService;

    public SedeController(ISedeService sedeService) {
        _sedeService = sedeService;
    }

    public async Task<IActionResult> Index(string? nombre, bool? activo) {
        ViewBag.FiltroNombre = nombre;
        ViewBag.FiltroActivo = activo;

        var sedes = await _sedeService.ObtenerTodasAsync(nombre, activo);

        return View(sedes);
    }

    public async Task<IActionResult> Details(int? id) {
        if (id == null) {
            return NotFound();
        }

        var sede = await _sedeService.ObtenerPorIdAsync(id.Value);

        if (sede == null) {
            return NotFound();
        }

        return View(sede);
    }

    public IActionResult Create() {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("SedeId,Nombre,Direccion,Telefono,Horario,Activo")] Sede sede) {
        if (!ModelState.IsValid) {
            return View(sede);
        }

        await _sedeService.CrearAsync(sede);

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int? id) {
        if (id == null) {
            return NotFound();
        }

        var sede = await _sedeService.ObtenerPorIdAsync(id.Value);

        if (sede == null) {
            return NotFound();
        }

        return View(sede);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("SedeId,Nombre,Direccion,Telefono,Horario,Activo")] Sede sede) {

        if (id != sede.SedeId) {
            return NotFound();
        }

        if (!ModelState.IsValid) {
            return View(sede);
        }

        var actualizada = await _sedeService.ActualizarAsync(sede);

        if (!actualizada) {
            return NotFound();
        }

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int? id) {
        if (id == null) {
            return NotFound();
        }

        var sede = await _sedeService.ObtenerPorIdAsync(id.Value);

        if (sede == null) {
            return NotFound();
        }

        return View(sede);
    }

    [HttpPost]
    [ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id) {
        var eliminada = await _sedeService.EliminarAsync(id);

        if (!eliminada) {
            return NotFound();
        }

        return RedirectToAction(nameof(Index));
    }
}
