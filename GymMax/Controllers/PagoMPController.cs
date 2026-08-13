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
            if (plan == null || !plan.Activo) return NotFound();

            return View(plan);
        }

        // POST: /PagoMP/IniciarPago
        // Crea la preferencia en MercadoPago y redirige al checkout
        [Authorize(Roles = "Cliente")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> IniciarPago(int planId)
        {
            var plan = await _context.Planes.FindAsync(planId);
            if (plan == null) return NotFound();

            // URL base de la app para las back_urls
            var baseUrl = $"{Request.Scheme}://{Request.Host}";

            var preferencia = new
            {
                items = new[]
                {
                    new
                    {
                        title       = plan.Nombre,
                        quantity    = 1,
                        unit_price  = plan.Precio,
                        currency_id = "PEN"
                    }
                },
                back_urls = new
                {
                    success = $"{baseUrl}/PagoMP/Exito?planId={planId}",
                    failure = $"{baseUrl}/PagoMP/Error?planId={planId}",
                    pending = $"{baseUrl}/PagoMP/Error?planId={planId}"
                },
                auto_return     = "approved",
                external_reference = planId.ToString()
            };

            var client  = _httpClientFactory.CreateClient("MercadoPago");
            var json    = JsonSerializer.Serialize(preferencia);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await client.PostAsync("/checkout/preferences", content);

            if (!response.IsSuccessStatusCode)
                return RedirectToAction("Error", new { planId });

            var body       = await response.Content.ReadAsStringAsync();
            var doc        = JsonDocument.Parse(body);
            var initPoint  = doc.RootElement.GetProperty("init_point").GetString();

            return Redirect(initPoint!);
        }

        // GET: /PagoMP/Exito
        // MercadoPago redirige aquí cuando el pago es aprobado
        [Authorize(Roles = "Cliente")]
        public async Task<IActionResult> Exito(int planId, string? payment_id, string? status)
        {
            // Si el pago no llegó aprobado, redirigimos a error
            if (status != "approved" || string.IsNullOrEmpty(payment_id))
                return RedirectToAction("Error", new { planId });

            // Verificar el pago consultando la API de MP
            var client   = _httpClientFactory.CreateClient("MercadoPago");
            var response = await client.GetAsync($"/v1/payments/{payment_id}");


            if (!response.IsSuccessStatusCode)
                return RedirectToAction("Error", new { planId });

            var body   = await response.Content.ReadAsStringAsync();
            var doc    = JsonDocument.Parse(body);
            var estado = doc.RootElement.GetProperty("status").GetString();

            if (estado != "approved")
                return RedirectToAction("Error", new { planId });

            // Obtener el UsuarioId desde los claims de sesión
            var usuarioIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(usuarioIdStr, out int usuarioId))
                return RedirectToAction("Error", new { planId });

            var plan = await _context.Planes.FindAsync(planId);
            if (plan == null) return NotFound();

            // Verificar que no tenga ya una suscripción activa al mismo plan
            var yaExiste = await _context.Suscripciones.AnyAsync(s =>
                s.UsuarioId == usuarioId &&
                s.PlanId    == planId    &&
                s.Estado    == EstadoSuscripcion.Activa);
            if (!yaExiste)
            {
                var hoy = DateOnly.FromDateTime(DateTime.Today);

                var suscripcion = new Suscripcion
                {
                    UsuarioId     = usuarioId,
                    PlanId        = planId,
                    PrecioPactado = plan.Precio,
                    FechaInicio   = hoy,
                    FechaFin      = hoy.AddDays(plan.DuracionDias),
                    Estado        = EstadoSuscripcion.Activa
                };
                _context.Suscripciones.Add(suscripcion);
                await _context.SaveChangesAsync();

                var pago = new Pago
                {
                    SuscripcionId = suscripcion.SuscripcionId,
                    Monto         = plan.Precio,
                    FechaPago     = DateTime.Now
                };
                _context.Pagos.Add(pago);
                await _context.SaveChangesAsync();
            }

            ViewBag.Plan      = plan;
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
    }
}
