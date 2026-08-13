using GymMax.Data;
using GymMax.Enums;
using GymMax.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace GymMax.Controllers
{
    public class PagoMPController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _config;
        public PagoMPController(AppDbContext context, IHttpClientFactory httpClientFactory, IConfiguration config)
        {
            _context = context;
            _httpClientFactory = httpClientFactory;
            _config = config;
        }
        // GET: /PagoMP/Confirmar/{planId}
        // Página de confirmación antes de pagar — requiere rol Cliente
        [Authorize(Roles = "Cliente")]
        public async Task<IActionResult> Confirmar(int planId)
        {
            var plan = await _context.Planes.FindAsync(planId);
            if (plan == null || !plan.Activo)
                return NotFound();
            var usuarioIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(usuarioIdStr, out int usuarioId))
                return RedirectToAction("Login", "Auth");
            var suscripcionActiva = await ObtenerSuscripcionActivaAsync(usuarioId);
            ViewBag.SuscripcionActiva = suscripcionActiva;
            return View(plan);
        }

        // POST: /PagoMP/IniciarPago
        // Crea la preferencia en MercadoPago y redirige al checkout
        [Authorize(Roles = "Cliente")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> IniciarPago(int planId, string tipoOperacion)
        {
            var plan = await _context.Planes.FindAsync(planId);
            if (plan == null || !plan.Activo)
                return NotFound();
            var usuarioIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(usuarioIdStr, out int usuarioId))
                return RedirectToAction("Login", "Auth");
            var suscripcionActiva = await ObtenerSuscripcionActivaAsync(usuarioId);
            // Determinar la operación realmente según la información de la base de datos
            if (suscripcionActiva == null)
            {
                tipoOperacion = "Nueva";
            }
            else if (suscripcionActiva.PlanId == planId)
            {
                tipoOperacion = "Renovacion";
            }
            else if (plan.Precio > suscripcionActiva.Plan.Precio)
            {
                tipoOperacion = "Upgrade";
            }
            else
            {
                // No permitimos comprar un plan mas barato
                return RedirectToAction("Confirmar", new { planId });
            }
            // Precio que realmente se cobrará
            decimal monto = plan.Precio;
            // Si es un cambio a un plan superior,
            // descontamos el valor proporcional de los días restantes.
            if (tipoOperacion == "CambioPlan")
            {
                var hoy = DateOnly.FromDateTime(DateTime.Today);
                var diasTotales =
                    suscripcionActiva!.FechaFin.DayNumber -
                    suscripcionActiva.FechaInicio.DayNumber;
                var diasRestantes =
                    suscripcionActiva.FechaFin.DayNumber -
                    hoy.DayNumber;
                if (diasTotales > 0 && diasRestantes > 0)
                {
                    var valorRestante = Math.Round(
                        suscripcionActiva.PrecioPactado *
                        diasRestantes /
                        diasTotales,
                        2
                    );
                    monto = Math.Max(
                        0,
                        Math.Round(plan.Precio - valorRestante, 2)
                    );
                }
            }
            // URL base de la aplicación
            var baseUrl = $"{Request.Scheme}://{Request.Host}";

            var preferencia = new
            {
                items = new[]
                {
            new
            {
                title = tipoOperacion == "CambioPlan"
                    ? $"Cambio a {plan.Nombre}"
                    : tipoOperacion == "Renovacion"
                        ? $"Renovación {plan.Nombre}"
                        : plan.Nombre,

                quantity = 1,
                unit_price = plan.Precio,
                currency_id = "PEN"
            }
        },
                back_urls = new
                {
                    success = $"{baseUrl}/PagoMP/Exito?planId={planId}",
                    failure = $"{baseUrl}/PagoMP/Error?planId={planId}",
                    pending = $"{baseUrl}/PagoMP/Error?planId={planId}"
                },
                auto_return = "approved",
                external_reference = $"{usuarioId}|{planId}|{tipoOperacion}"
            };
            var client = _httpClientFactory.CreateClient("MercadoPago");
            var json = JsonSerializer.Serialize(preferencia);
            var content = new StringContent(
                json,
                Encoding.UTF8,
                "application/json"
            );
            var response = await client.PostAsync(
                "/checkout/preferences",
                content
            );
            if (!response.IsSuccessStatusCode)
                return RedirectToAction("Error", new { planId });
            var body = await response.Content.ReadAsStringAsync();
            var doc = JsonDocument.Parse(body);
            var initPoint =
                doc.RootElement
                    .GetProperty("init_point")
                    .GetString();
            return Redirect(initPoint!);
        }

        // GET: /PagoMP/Exito
        // MercadoPago redirige aquí cuando el pago es aprobado
        [Authorize(Roles = "Cliente")]
        public async Task<IActionResult> Exito(int planId, string? payment_id, string? status)
        {
            // Verificar que MercadoPago haya aprobado el pago
            if (status != "approved" || string.IsNullOrEmpty(payment_id))
                return RedirectToAction("Error", new { planId });

            var client = _httpClientFactory.CreateClient("MercadoPago");

            var response = await client.GetAsync($"/v1/payments/{payment_id}");

            if (!response.IsSuccessStatusCode)
                return RedirectToAction("Error", new { planId });

            var body = await response.Content.ReadAsStringAsync();
            var doc = JsonDocument.Parse(body);

            var estado = doc.RootElement.GetProperty("status").GetString();

            if (estado != "approved")
                return RedirectToAction("Error", new { planId });

            // Obtener external_reference
            var externalReference =
                doc.RootElement.GetProperty("external_reference").GetString();
            if (string.IsNullOrEmpty(externalReference))
                return RedirectToAction("Error", new { planId });
            var partes = externalReference.Split('|');

            if (partes.Length != 3)
                return RedirectToAction("Error", new { planId });

            if (!int.TryParse(partes[0], out int usuarioId))
                return RedirectToAction("Error", new { planId });

            if (!int.TryParse(partes[1], out int planIdReferencia))
                return RedirectToAction("Error", new { planId });

            var tipoOperacion = partes[2];

            // Seguridad: el plan recibido por URL debe coincidir
            if (planId != planIdReferencia)
                return RedirectToAction("Error", new { planId });

            // Obtener plan
            var plan = await _context.Planes.FindAsync(planId);

            if (plan == null || !plan.Activo)
                return NotFound();

            // Buscar suscripción activa actual
            var suscripcionActiva = await ObtenerSuscripcionActivaAsync(usuarioId);

            var hoy = DateOnly.FromDateTime(DateTime.Today);
            // NUEVA SUSCRIPCIÓN
            if (tipoOperacion == "Nueva")
            {
                // Protección adicional:
                // si por alguna razón ya tiene una activa, no crear otra.
                if (suscripcionActiva != null)
                    return RedirectToAction("Index", "Cliente");

                var suscripcion = new Suscripcion
                {
                    UsuarioId = usuarioId,
                    PlanId = planId,
                    PrecioPactado = plan.Precio,
                    FechaInicio = hoy,
                    FechaFin = hoy.AddDays(plan.DuracionDias),
                    Estado = EstadoSuscripcion.Activa
                };

                _context.Suscripciones.Add(suscripcion);

                await _context.SaveChangesAsync();

                var pago = new Pago
                {
                    SuscripcionId = suscripcion.SuscripcionId,
                    Monto = plan.Precio,
                    FechaPago = DateTime.Now
                };

                _context.Pagos.Add(pago);

                await _context.SaveChangesAsync();
            }
            // RENOVACIÓN
            else if (tipoOperacion == "Renovacion")
            {
                // Debe existir una suscripción activa
                if (suscripcionActiva == null)
                    return RedirectToAction("Error", new { planId });

                // La renovación debe ser del mismo plan
                if (suscripcionActiva.PlanId != planId)
                    return RedirectToAction("Error", new { planId });

                // Extender desde la fecha de vencimiento actual
                suscripcionActiva.FechaFin =
                    suscripcionActiva.FechaFin.AddDays(plan.DuracionDias);

                var pago = new Pago
                {
                    SuscripcionId = suscripcionActiva.SuscripcionId,
                    Monto = plan.Precio,
                    FechaPago = DateTime.Now
                };

                _context.Pagos.Add(pago);

                await _context.SaveChangesAsync();
            }
            // UPGRADE
            else if (tipoOperacion == "Upgrade")
            {
                // Debe existir una suscripción activa
                if (suscripcionActiva == null)
                    return RedirectToAction("Error", new { planId });
                // No permitir que Upgrade sea realmente el mismo plan
                if (suscripcionActiva.PlanId == planId)
                    return RedirectToAction("Error", new { planId });
                // Cancelar la suscripción anterior
                suscripcionActiva.Estado = EstadoSuscripcion.Cancelada;
                // Crear la nueva suscripción
                var nuevaSuscripcion = new Suscripcion
                {
                    UsuarioId = usuarioId,
                    PlanId = planId,
                    PrecioPactado = plan.Precio,
                    FechaInicio = hoy,
                    FechaFin = hoy.AddDays(plan.DuracionDias),
                    Estado = EstadoSuscripcion.Activa
                };
                _context.Suscripciones.Add(nuevaSuscripcion);

                await _context.SaveChangesAsync();

                // Registrar el pago de la nueva suscripción
                var pago = new Pago
                {
                    SuscripcionId = nuevaSuscripcion.SuscripcionId,
                    Monto = plan.Precio,
                    FechaPago = DateTime.Now
                };
                _context.Pagos.Add(pago);
                await _context.SaveChangesAsync();
            }
            else
            {
                return RedirectToAction("Error", new { planId });
            }
            ViewBag.Plan = plan;
            ViewBag.PaymentId = payment_id;
            return View();
        }
        // GET: /PagoMP/Error
        [Authorize(Roles = "Cliente")]
        public async Task<IActionResult> Error(int planId)
        {
            ViewBag.Plan = await _context.Planes.FindAsync(planId);
            return View();
        }
        //NUEVO METODO PARA OBTENER SUSCRIPCIONES ACTIVAS DE UN USUARIO:
        private async Task<Suscripcion?> ObtenerSuscripcionActivaAsync(int usuarioId)
        {
            return await _context.Suscripciones
                .Include(s => s.Plan)
                .FirstOrDefaultAsync(s =>
                    s.UsuarioId == usuarioId &&
                    s.Estado == EstadoSuscripcion.Activa);
        }
    }
}
