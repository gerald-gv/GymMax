using GymMax.Data;
using GymMax.Domain.Entities;
using GymMax.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;


namespace GymMax.Services.Auth {
    public class AuthService : IAuthService {

        private readonly AppDbContext _context;
        private readonly PasswordHasher<Usuario> _passwordHasher;

        public AuthService(AppDbContext context) {
            _context = context;
            _passwordHasher = new PasswordHasher<Usuario>();
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
    }
}
