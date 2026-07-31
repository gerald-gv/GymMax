
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GymMax.Models;
using GymMax.Services.Planes;

[Authorize(Roles = "Administrador")]
public class PlanController : Controller
{
    private readonly IPlanesService _planesService;

    public PlanController(IPlanesService planesService) {
        _planesService = planesService;
    }

    // GET: Plan
    public async Task<IActionResult> Index(
        string? nombre,
        decimal? precioMin,
        decimal? precioMax
        ) {
        var planes = await _planesService.ObtenerTodosAsync(
            nombre,
            precioMin,
            precioMax);

        ViewBag.FiltroNombre = nombre;
        ViewBag.FiltroPrecioMin = precioMin;
        ViewBag.FiltroPrecioMax = precioMax;

        return View(planes);
    }

    // GET: Plan/Details/5
    public async Task<IActionResult> Details(int? id) {
        if (id == null)
            return NotFound();

        var plan = await _planesService.ObtenerPorIdAsync(id.Value);

        if (plan == null)
            return NotFound();

        return View(plan);
    }

    // GET: Plan/Create
    public IActionResult Create() {
        return View();
    }

    // POST: Plan/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("PlanId,Nombre,Descripcion,DuracionDias,Precio,Activo")] Plan plan) {
        if (!ModelState.IsValid)
            return View(plan);

        await _planesService.CrearAsync(plan);

        return RedirectToAction(nameof(Index));
    }

    // GET: Plan/Edit/5
    public async Task<IActionResult> Edit(int? id) {
        if (id == null)
            return NotFound();

        var plan = await _planesService.ObtenerPorIdAsync(id.Value);

        if (plan == null)
            return NotFound();

        return View(plan);
    }

    // POST: Plan/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit( int id, [Bind("PlanId,Nombre,Descripcion,DuracionDias,Precio,Activo")] Plan plan) {
        
        if (id != plan.PlanId) return NotFound();

        if (!ModelState.IsValid) return View(plan);

        try {
            await _planesService.ActualizarAsync(plan);
        } catch (DbUpdateConcurrencyException) {
            if (!await _planesService.ExisteAsync(plan.PlanId))
                return NotFound();

            throw;
        }

        return RedirectToAction(nameof(Index));
    }

    // GET: Plan/Delete/5
    public async Task<IActionResult> Delete(int? id) {
        if (id == null) return NotFound();

        var plan = await _planesService.ObtenerPorIdAsync(id.Value);

        if (plan == null) return NotFound();

        return View(plan);
    }

    // POST: Plan/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id) {
        await _planesService.EliminarAsync(id);
        return RedirectToAction(nameof(Index));
    }
}