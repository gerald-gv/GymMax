using GymMax.Data;
using GymMax.Domain.Entities;
using GymMax.DTOs;
using GymMax.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace GymMax.Services.Perfil
{
    public class PerfilService : IPerfilService
    {
        private readonly AppDbContext _context;
        private readonly IPasswordHasher<Usuario> _passwordHasher; // el mismo que usa IAuthService

        public PerfilService(AppDbContext context, IPasswordHasher<Usuario> passwordHasher)
        {
            _context = context;
            _passwordHasher = passwordHasher;
        }

        // ---------------------------------------------------------------
        // Obtiene los datos del perfil (solo lectura)
        // ---------------------------------------------------------------
        public async Task<PerfilViewDTO?> ObtenerPerfilAsync(int usuarioId)
        {
            var u = await _context.Usuarios.AsNoTracking()
                .FirstOrDefaultAsync(x => x.UsuarioId == usuarioId);

            if (u == null) return null;

            return new PerfilViewDTO
            {
                UsuarioId = u.UsuarioId,
                Nombres = u.Nombres,
                Apellidos = u.Apellidos,
                Dni = u.Dni,
                Email = u.Email,
                Telefono = u.Telefono,
                FechaNacimiento = u.FechaNacimiento,
                CodigoMembresia = u.CodigoMembresia,
                FechaRegistro = u.FechaRegistro,
                Estado = u.Estado.ToString()
            };
        }

        // ---------------------------------------------------------------
        // Obtiene los datos actuales para prellenar el formulario de edición
        // ---------------------------------------------------------------
        public async Task<EditarPerfilDTO?> ObtenerParaEditarAsync(int usuarioId)
        {
            var u = await _context.Usuarios.AsNoTracking()
                .FirstOrDefaultAsync(x => x.UsuarioId == usuarioId);

            if (u == null) return null;

            return new EditarPerfilDTO
            {
                Email = u.Email,
                Telefono = u.Telefono
            };
        }

        // ---------------------------------------------------------------
        // Actualiza email y teléfono. Solo estos dos campos se modifican
        // en la entidad, sin importar qué más traiga el ViewModel.
        // ---------------------------------------------------------------
        public async Task<ResultadoActualizacion> ActualizarAsync(int usuarioId, EditarPerfilDTO model)
        {
            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(x => x.UsuarioId == usuarioId);

            if (usuario == null)
                return ResultadoActualizacion.UsuarioNoEncontrado;

            var emailEnUso = await _context.Usuarios
                .AnyAsync(x => x.Email == model.Email && x.UsuarioId != usuarioId);

            if (emailEnUso)
                return ResultadoActualizacion.EmailYaExiste;

            usuario.Email = model.Email;
            usuario.Telefono = model.Telefono;

            await _context.SaveChangesAsync();
            return ResultadoActualizacion.Exito;
        }

        // ---------------------------------------------------------------
        // Verifica la contraseña actual y, si es correcta, la reemplaza
        // por el hash de la nueva.
        // ---------------------------------------------------------------
        public async Task<ResultadoCambioPassword> CambiarPasswordAsync(int usuarioId, CambiarPasswordDTO model)
        {
            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(x => x.UsuarioId == usuarioId);
            if (usuario == null)
                return ResultadoCambioPassword.UsuarioNoEncontrado;
            var resultadoVerificacion = _passwordHasher.VerifyHashedPassword(
                usuario, usuario.PasswordHash, model.PasswordActual);
            if (resultadoVerificacion == PasswordVerificationResult.Failed)
                return ResultadoCambioPassword.PasswordActualIncorrecta;
            usuario.PasswordHash = _passwordHasher.HashPassword(usuario, model.PasswordNueva);
            await _context.SaveChangesAsync();
            return ResultadoCambioPassword.Exito;
        }
    }
}
