using GymMax.DTOs;
using GymMax.Enums;
using GymMax.Services;
using GymMax.Services.Perfil;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace GymMax.Controllers
{
    [Authorize]
    public class PerfilController : Controller
    {
        private readonly IPerfilService _perfilService;

        public PerfilController(IPerfilService perfilService)
        {
            _perfilService = perfilService;
        }

        // ---------------------------------------------------------------
        // GET: /Perfil
        // Muestra los datos del perfil (solo lectura).
        // ---------------------------------------------------------------
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var perfil = await _perfilService.ObtenerPerfilAsync(ObtenerUsuarioId());
            if (perfil == null) return NotFound();

            return View(perfil);
        }

        // ---------------------------------------------------------------
        // GET: /Perfil/Editar
        // Muestra el formulario para editar email y teléfono.
        // ---------------------------------------------------------------
        [HttpGet]
        public async Task<IActionResult> Editar()
        {
            var usuario = await _perfilService.ObtenerParaEditarAsync(ObtenerUsuarioId());
            if (usuario == null) return NotFound();

            return View(usuario);
        }

        // ---------------------------------------------------------------
        // POST: /Perfil/Editar
        // Procesa la actualización de email y teléfono.
        // ---------------------------------------------------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(EditarPerfilDTO model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var resultado = await _perfilService.ActualizarAsync(ObtenerUsuarioId(), model);

            if (resultado == ResultadoActualizacion.EmailYaExiste)
            {
                ModelState.AddModelError(nameof(model.Email), "Ese email ya está en uso por otra cuenta.");
                return View(model);
            }

            TempData["PerfilActualizado"] = "Tu perfil se actualizó correctamente.";
            return RedirectToAction(nameof(Index));
        }

        // ---------------------------------------------------------------
        // GET: /Perfil/CambiarPassword
        // Muestra el formulario para cambiar la contraseña.
        // ---------------------------------------------------------------
        [HttpGet]
        public IActionResult CambiarPassword()
        {
            return View(new CambiarPasswordDTO());
        }

        // ---------------------------------------------------------------
        // POST: /Perfil/CambiarPassword
        // Verifica la contraseña actual y aplica la nueva.
        // Al finalizar, cierra la sesión para forzar un nuevo login.
        // ---------------------------------------------------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CambiarPassword(CambiarPasswordDTO model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var resultado = await _perfilService.CambiarPasswordAsync(ObtenerUsuarioId(), model);

            if (resultado == ResultadoCambioPassword.PasswordActualIncorrecta)
            {
                ModelState.AddModelError(nameof(model.PasswordActual), "La contraseña actual es incorrecta.");
                return View(model);
            }

            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            TempData["PasswordActualizada"] = "Tu contraseña se actualizó. Inicia sesión nuevamente.";
            return RedirectToAction("Login", "Auth");
        }

        // ---------------------------------------------------------------
        // ObtenerUsuarioId()
        // Extrae el UsuarioId desde los Claims de la cookie autenticada.
        // Nunca se toma de un parámetro de la request (evita IDOR).
        // ---------------------------------------------------------------
        private int ObtenerUsuarioId()
        {
            var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.Parse(claim!);
        }
    }
}
