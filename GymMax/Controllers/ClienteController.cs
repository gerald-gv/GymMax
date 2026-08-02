using GymMax.Data;
using GymMax.Enums;
using GymMax.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace GymMax.Controllers
{
    public class ClienteController : Controller
    {
        private readonly AppDbContext _context;

        public ClienteController(AppDbContext context)
        {
            _context = context;
        }

        // GET: /Cliente/Index — portal del cliente con sus suscripciones
        [Authorize(Roles = "Cliente")]
        public async Task<IActionResult> Index()
        {
            var usuarioIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(usuarioIdStr, out int usuarioId))
                return RedirectToAction("Login", "Auth");

            var suscripciones = await _context.Suscripciones
                .Include(s => s.Plan)
                .Where(s => s.UsuarioId == usuarioId)
                .OrderByDescending(s => s.FechaInicio)
                .ToListAsync();

            // Calcular reembolso proporcional para cada suscripción activa
            var hoy = DateOnly.FromDateTime(DateTime.Today);
            var reembolsos = suscripciones.ToDictionary(
                s => s.SuscripcionId,
                s => {
                    if (s.Estado != EstadoSuscripcion.Activa || s.FechaFin < hoy)
                        return 0m;
                    var diasTotales   = s.FechaFin.DayNumber - s.FechaInicio.DayNumber;
                    var diasRestantes = s.FechaFin.DayNumber - hoy.DayNumber;
                    return diasTotales > 0
                        ? Math.Round(s.PrecioPactado * diasRestantes / diasTotales, 2)
                        : 0m;
                });

            ViewBag.Reembolsos = reembolsos;
            return View(suscripciones);
        }

        // POST: /Cliente/CancelarSuscripcion/{id}
        // Cancela la suscripción y registra el reembolso proporcional como pago negativo.
        [Authorize(Roles = "Cliente")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelarSuscripcion(int id)
        {
            var usuarioIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(usuarioIdStr, out int usuarioId))
                return RedirectToAction("Login", "Auth");

            var suscripcion = await _context.Suscripciones
                .Include(s => s.Plan)
                .FirstOrDefaultAsync(s => s.SuscripcionId == id && s.UsuarioId == usuarioId);

            if (suscripcion == null)
                return NotFound();

            if (suscripcion.Estado != EstadoSuscripcion.Activa)
            {
                TempData["Error"] = "Solo puedes cancelar suscripciones activas.";
                return RedirectToAction(nameof(Index));
            }

            // Calcular reembolso proporcional por días restantes
            var hoy           = DateOnly.FromDateTime(DateTime.Today);
            var diasTotales   = suscripcion.FechaFin.DayNumber - suscripcion.FechaInicio.DayNumber;
            var diasRestantes = suscripcion.FechaFin.DayNumber - hoy.DayNumber;
            var montoReembolso = diasTotales > 0
                ? Math.Round(suscripcion.PrecioPactado * diasRestantes / diasTotales, 2)
                : 0m;

            // Registrar reembolso como pago negativo (descuenta del dashboard)
            if (montoReembolso > 0)
            {
                _context.Pagos.Add(new Pago
                {
                    SuscripcionId = suscripcion.SuscripcionId,
                    Monto         = -montoReembolso,
                    FechaPago     = DateTime.Now
                });
            }

            suscripcion.Estado = EstadoSuscripcion.Cancelada;
            await _context.SaveChangesAsync();

            TempData["Exito"] = montoReembolso > 0
                ? $"Suscripción al plan \"{suscripcion.Plan.Nombre}\" cancelada. Reembolso de S/ {montoReembolso:N2} registrado."
                : $"Suscripción al plan \"{suscripcion.Plan.Nombre}\" cancelada.";

            return RedirectToAction(nameof(Index));
        }

        // GET: /Cliente/Planes — vista pública de planes (cualquiera puede verla)
        public async Task<IActionResult> Planes()
        {
            var planes = await _context.Planes.Where(p => p.Activo).ToListAsync();
            return View(planes);
        }
    }
}
