using GymMax.Data;
using GymMax.Enums;
using GymMax.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace GymMax.Controllers
{
    public class AuthController : Controller
    {
        private readonly AppDbContext _context;

        public AuthController(AppDbContext context)
        {
            _context = context;
        }

        // ---------------------------------------------------------------
        // GET: /Auth/Login
        // Muestra el formulario de login.
        // Si el usuario ya tiene una sesión activa (cookie válida),
        // lo redirige directamente según su rol sin mostrar el formulario.
        // ---------------------------------------------------------------
        public IActionResult Login()
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectSegunRol();

            return View();
        }

        // ---------------------------------------------------------------
        // POST: /Auth/Login
        // Procesa el formulario de login.
        //
        // Flujo completo:
        //   1. Valida que el modelo (Email + Password) sea correcto.
        //   2. Busca el usuario en la BD por email, incluyendo su Rol.
        //   3. Verifica la contraseña usando PasswordHasher de ASP.NET Core.
        //   4. Si todo es correcto, construye los Claims del usuario.
        //   5. Crea la identidad y el principal con esos Claims.
        //   6. Firma la cookie de sesión (inicia sesión).
        //   7. Redirige al área correspondiente según el rol.
        // ---------------------------------------------------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            // Paso 1 — Validación del modelo
            // Si el formulario tiene errores (email vacío, formato inválido, etc.),
            // se devuelve la vista con los mensajes de error sin consultar la BD.
            if (!ModelState.IsValid)
                return View(model);

            // Paso 2 — Búsqueda del usuario por email
            // Se usa .Include(u => u.Rol) para cargar la relación con la tabla Rol
            // en la misma consulta (evita una segunda consulta a la BD).
            // Si no existe ningún usuario con ese email, se muestra error genérico
            // (no decir "el email no existe" por seguridad).
            var usuario = await _context.Usuarios
                .Include(u => u.Rol)
                .FirstOrDefaultAsync(u => u.Email == model.Email);

            if (usuario == null)
            {
                ModelState.AddModelError(string.Empty, "Email o contraseña incorrectos.");
                return View(model);
            }

            // Paso 3 — Verificación de contraseña con PasswordHasher
            // ASP.NET Core nunca guarda contraseñas en texto plano.
            // Al crear el usuario se guardó un hash (cadena cifrada) en la BD.
            // VerifyHashedPassword compara la contraseña que escribió el usuario
            // contra el hash almacenado usando el mismo algoritmo (PBKDF2).
            // Resultado posible:
            //   - PasswordVerificationResult.Success  → contraseña correcta
            //   - PasswordVerificationResult.Failed   → contraseña incorrecta
            //   - PasswordVerificationResult.SuccessRehashNeeded → correcta pero
            //     el hash es antiguo y debería regenerarse (no manejamos esto aquí)
            var hasher = new PasswordHasher<GymMax.Domain.Entities.Usuario>();
            var resultado = hasher.VerifyHashedPassword(usuario, usuario.PasswordHash, model.Password);

            if (resultado == PasswordVerificationResult.Failed)
            {
                ModelState.AddModelError(string.Empty, "Email o contraseña incorrectos.");
                return View(model);
            }

            // Paso 4 — Construcción de los Claims
            // Un Claim es un par clave-valor que describe al usuario autenticado.
            // Se almacenan dentro de la cookie de sesión (encriptada).
            // En cualquier parte de la app se pueden leer con User.FindFirstValue(...)
            // o con User.IsInRole(...).
            //
            // ClaimTypes estándar de .NET:
            //   - NameIdentifier → ID único del usuario (para consultas futuras a la BD)
            //   - Name           → Nombre completo (se muestra en la UI con @User.Identity.Name)
            //   - Email          → Email del usuario
            //   - Role           → Rol del usuario — ASP.NET lo usa para [Authorize(Roles="...")]
            //
            // Si en el futuro necesitas agregar más datos a la sesión (por ejemplo,
            // la sede asignada, el código de membresía, etc.), agrégalos aquí como
            // new Claim("sede", usuario.SedeId.ToString()) y léelos con
            // User.FindFirstValue("sede") desde cualquier controlador o vista.
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, usuario.UsuarioId.ToString()),
                new Claim(ClaimTypes.Name,  $"{usuario.Nombres} {usuario.Apellidos}"),
                new Claim(ClaimTypes.Email, usuario.Email),
                new Claim(ClaimTypes.Role,  usuario.Rol!.Nombre)
            };

            // Paso 5 — Construcción de la identidad y el principal
            // ClaimsIdentity agrupa los Claims bajo un esquema de autenticación.
            // El esquema "Cookies" le dice a ASP.NET que esta identidad se gestiona
            // mediante cookies (configurado en Program.cs con AddAuthentication).
            //
            // ClaimsPrincipal es el "usuario" que representa a la persona autenticada.
            // Puede tener múltiples identidades (poco común, pero posible).
            // Es lo que queda disponible como "User" en los controladores y vistas.
            var identidad  = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal  = new ClaimsPrincipal(identidad);

            // Paso 6 — Firma de la cookie (inicio de sesión)
            // SignInAsync serializa el ClaimsPrincipal, lo encripta y lo guarda
            // como una cookie en el navegador del usuario.
            // A partir de aquí, cada request que haga el usuario enviará esa cookie
            // automáticamente, y ASP.NET la desencripta para reconstruir el User.
            // La duración y configuración de la cookie se define en Program.cs
            // (ExpireTimeSpan, SlidingExpiration, LoginPath, etc.).
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            // Paso 7 — Redirección según el rol
            return RedirectSegunRol();
        }

        // ---------------------------------------------------------------
        // POST: /Auth/Logout
        // Cierra la sesión del usuario.
        // SignOutAsync elimina la cookie del navegador e invalida la sesión.
        // Solo acepta POST (con token anti-falsificación) para evitar que
        // un enlace malicioso pueda cerrar la sesión del usuario sin su consentimiento.
        // ---------------------------------------------------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }

        // ---------------------------------------------------------------
        // RedirectSegunRol()
        // Método privado que lee el Claim de rol del usuario autenticado
        // y lo redirige al área correspondiente.
        //
        // User.FindFirstValue(ClaimTypes.Role) lee directamente el Claim
        // desde la cookie ya firmada (no hace consulta a la BD).
        //
        // El bloque ?? (fallback) solo se ejecuta si el claim de rol no
        // está disponible todavía (por ejemplo, justo al llamarlo desde
        // Login() antes de que la cookie esté firmada). En ese caso hace
        // una consulta a la BD para obtener el rol por email.
        //
        // nameof(RolUsuario.Administrador) devuelve el string "Administrador",
        // que debe coincidir exactamente con el valor guardado en la tabla Rol.
        //
        // Para agregar una nueva redirección por rol en el futuro, añade
        // una línea al switch:
        //   nameof(RolUsuario.NuevoRol) => RedirectToAction("Index", "NuevoRolController"),
        // ---------------------------------------------------------------
        private IActionResult RedirectSegunRol()
        {
            var rol = User.FindFirstValue(ClaimTypes.Role)
                      ?? _context.Usuarios
                             .Include(u => u.Rol)
                             .FirstOrDefault(u => u.Email == User.FindFirstValue(ClaimTypes.Email))
                             ?.Rol?.Nombre;

            return rol switch
            {
                nameof(RolUsuario.Administrador) => RedirectToAction("Index", "Home"),
                nameof(RolUsuario.Coach)         => RedirectToAction("Index", "Home"),
                nameof(RolUsuario.Cliente)       => RedirectToAction("Index", "Cliente"),
                _                                => RedirectToAction("Login")
            };
        }
    }
}