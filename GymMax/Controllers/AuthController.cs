using GymMax.Data;
using GymMax.Enums;
using GymMax.Services.Auth;
using GymMax.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace GymMax.Controllers
{
    public class AuthController : Controller {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService) {
            _authService = authService;
        }

        // ---------------------------------------------------------------
        // GET: /Auth/Login
        // Muestra el formulario de login.
        // Si el usuario ya tiene una sesión activa (cookie válida),
        // lo redirige directamente según su rol sin mostrar el formulario.
        //
        // [AllowAnonymous] permite que un usuario no autenticado pueda
        // acceder al login sin ser redirigido nuevamente a esta misma ruta.
        // ---------------------------------------------------------------
        [AllowAnonymous]
        [HttpGet]
        public IActionResult Login() {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectSegunRol();

            return View();
        }

        // ---------------------------------------------------------------
        // POST: /Auth/Login
        // Procesa el formulario de login.
        //
        // Flujo completo:
        //   1. Valida el formulario.
        //   2. Solicita al servicio autenticar al usuario.
        //   3. Si las credenciales son inválidas devuelve error.
        //   4. Firma la cookie de autenticación.
        //   5. Redirige según el rol.
        // ---------------------------------------------------------------
        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model) {
            // Paso 1 — Validación del modelo
            if (!ModelState.IsValid)
                return View(model);

            // Paso 2 — Delegar la autenticación al servicio
            var principal = await _authService.AutenticarAsync(model);

            // Paso 3 — Credenciales inválidas
            if (principal == null) {
                ModelState.AddModelError(
                    string.Empty,
                    "Email o contraseña incorrectos."
                );

                return View(model);
            }

            // Paso 4 — Firma de la cookie
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal
            );

            // Paso 5 — Redirección según el rol
            return RedirectSegunRol();
        }

        // ---------------------------------------------------------------
        // POST: /Auth/Logout
        // Cierra la sesión del usuario.
        // ---------------------------------------------------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout() {
            await HttpContext.SignOutAsync(
                CookieAuthenticationDefaults.AuthenticationScheme
            );

            return RedirectToAction(
                "Index",
                "Home"
            );
        }

        // ---------------------------------------------------------------
        // RedirectSegunRol()
        // Lee el rol desde los Claims almacenados en la cookie.
        // ---------------------------------------------------------------
        private IActionResult RedirectSegunRol() {
            var rol = User.FindFirstValue(ClaimTypes.Role);

            return rol switch {
                nameof(RolUsuario.Administrador) => RedirectToAction("Index", "Dashboard"),
                nameof(RolUsuario.Coach)         => RedirectToAction("Index", "Home"),
                nameof(RolUsuario.Cliente)       => RedirectToAction("Index", "Cliente"),
                _                                => RedirectToAction("Login", "Auth")
            };
        }

        // GET: /Auth/AccesoDenegado
        [AllowAnonymous]
        public IActionResult AccesoDenegado()
        {
            return View();
        }


        // GET: /Auth/Register

        [AllowAnonymous]
        [HttpGet]
        public IActionResult Register() {
            if (User.Identity?.IsAuthenticated == true) {
                return RedirectSegunRol();
            }

            return View();
        }

        // POST: /Auth/Register
        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegistroViewModel model) {
            if (!ModelState.IsValid) {
                return View(model);
            }

            var resultado = await _authService.RegistrarAsync(model);
            if (resultado == ResultadoRegistro.EmailYaExiste) {
                ModelState.AddModelError(nameof(model.Email), "Ya existe una cuenta registrada con este email");
                return View(model);
            }


            if (resultado == ResultadoRegistro.DniYaExiste) {
                ModelState.AddModelError(nameof(model.Dni), "Ya existe una cuenta registrada con este DNI.");
                return View(model);
            }

            // Login automatico luego del registro
            var loginModel = new LoginViewModel { Email = model.Email, Password = model.Password };
            var principal = await _authService.AutenticarAsync(loginModel);

            if(principal != null) {
                await HttpContext.SignInAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme,
                        principal
                    );

                return RedirectSegunRol();
            }

            return RedirectToAction("Login");
        }

    }
}