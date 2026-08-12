using GymMax.Data;
using GymMax.Domain.Entities;
using GymMax.Enums;
using GymMax.Services.Usuarios;
using GymMax.ViewModels;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;


namespace GymMax.Services.Auth {
    public class AuthService : IAuthService {

        private readonly AppDbContext _context;
        private readonly PasswordHasher<Usuario> _passwordHasher;
        private readonly IUsuarioService _usuarioService;

        public AuthService(AppDbContext context, IUsuarioService usuarioService) {
            _context = context;
            _passwordHasher = new PasswordHasher<Usuario>();
            _usuarioService = usuarioService;
        }

        public async Task<ClaimsPrincipal?> AutenticarAsync(LoginViewModel model) {
            var usuario = await _context.Usuarios
                .Include(u => u.Rol)
                .FirstOrDefaultAsync(u => u.Email == model.Email);

            if (usuario == null)
                return null;

            var resultado = _passwordHasher.VerifyHashedPassword(
                usuario,
                usuario.PasswordHash,
                model.Password
            );

            if (resultado == PasswordVerificationResult.Failed)
                return null;

            var claims = new List<Claim>
            {
                new Claim(
                    ClaimTypes.NameIdentifier,
                    usuario.UsuarioId.ToString()
                ),

                new Claim(
                    ClaimTypes.Name,
                    $"{usuario.Nombres} {usuario.Apellidos}"
                ),

                new Claim(
                    ClaimTypes.Email,
                    usuario.Email
                ),

                new Claim(
                    ClaimTypes.Role,
                    usuario.Rol!.Nombre
                )
            };

            var identidad = new ClaimsIdentity(
                claims,
                CookieAuthenticationDefaults.AuthenticationScheme
            );

            return new ClaimsPrincipal(identidad);
        }

        public async Task<ResultadoRegistro> RegistrarAsync(RegistroViewModel model) {
            // Paso 1 — Verificar que el email no esté ya registrado
            var emailExiste = await _context.Usuarios.AnyAsync(u => u.Email == model.Email);
            if (emailExiste)
                return ResultadoRegistro.EmailYaExiste;

            // Paso 2 — Verificar que el DNI no esté ya registrado
            var dniExiste = await _context.Usuarios.AnyAsync(u => u.Dni == model.Dni);
            if (dniExiste)
                return ResultadoRegistro.DniYaExiste;

            // Paso 3 — Buscar el rol "Cliente"
            var rolCliente = await _context.Roles
                .FirstOrDefaultAsync(r => r.Nombre == nameof(RolUsuario.Cliente));

            if (rolCliente == null)
                throw new InvalidOperationException("El rol 'Cliente' no está configurado en la base de datos.");

            // Paso 4 — Crear el usuario
            var usuario = new Usuario {
                Nombres = model.Nombres,
                Apellidos = model.Apellidos,
                Dni = model.Dni,
                Email = model.Email,
                Telefono = model.Telefono,
                FechaNacimiento = model.FechaNacimiento,
                RolId = rolCliente.RolId,
                FechaRegistro = DateTime.UtcNow,
                Estado = EstadoUsuario.Activo,
                CodigoMembresia = _usuarioService.GenerarCodigoMembresia()
            };

            usuario.PasswordHash = _passwordHasher.HashPassword(usuario, model.Password);

            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();

            return ResultadoRegistro.Exitoso;
        }
    }
}
