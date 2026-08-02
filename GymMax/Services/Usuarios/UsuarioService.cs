using GymMax.Data;
using GymMax.Domain.Entities;
using GymMax.Enums;
using GymMax.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace GymMax.Services.Usuarios {
    public class UsuarioService : IUsuarioService {

        private readonly AppDbContext _context;

        public UsuarioService(AppDbContext context) {
            _context = context;
        }

        public async Task<List<Usuario>> GetAllAsync(
            string? nombre,
            int? rolId,
            EstadoUsuario? estado,
            DateOnly? fechaDesde,
            DateOnly? fechaHasta
            ) {
            var query = _context.Usuarios
                .Include(u => u.Rol)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(nombre)) {
                query = query.Where(u =>
                    u.Nombres.Contains(nombre) ||
                    u.Apellidos.Contains(nombre));
            }

            if (rolId.HasValue) {
                query = query.Where(u => u.RolId == rolId);
            }

            if (estado.HasValue) {
                query = query.Where(u => u.Estado == estado);
            }

            if (fechaDesde.HasValue) {
                query = query.Where(u =>
                    DateOnly.FromDateTime(u.FechaRegistro) >= fechaDesde.Value);
            }

            if (fechaHasta.HasValue) {
                query = query.Where(u =>
                    DateOnly.FromDateTime(u.FechaRegistro) <= fechaHasta.Value);
            }

            return await query.ToListAsync();
        }

        public async Task<Usuario?> GetByIdAsync(int id) {
            return await _context.Usuarios
                .Include(u => u.Rol)
                .FirstOrDefaultAsync(u => u.UsuarioId == id);
        }

        public async Task<Usuario?> GetForEditAsync(int id) {
            return await _context.Usuarios.FindAsync(id);
        }

        public async Task<bool> CreateAsync( Usuario usuario, string password ) {
            usuario.FechaRegistro = DateTime.Now;

            var passwordHasher = new PasswordHasher<Usuario>();

            usuario.PasswordHash = passwordHasher.HashPassword(usuario, password);

            if (usuario.RolId == (int)RolUsuario.Cliente) {
                usuario.CodigoMembresia = GenerarCodigoMembresia();
            }

            _context.Usuarios.Add(usuario);

            if (usuario.RolId == (int)RolUsuario.Coach) {
                var coach = new Coach {
                    Usuario = usuario,
                    FechaIngreso = DateOnly.FromDateTime(DateTime.Now),
                    Activo = usuario.Estado == EstadoUsuario.Activo
                };

                _context.Coaches.Add(coach);
            }
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateAsync(
            Usuario usuarioInput,
            string? nuevaPassword
            ) {
            var usuarioDb = await _context.Usuarios.FindAsync(usuarioInput.UsuarioId);

            if (usuarioDb == null) {
                return false;
            }

            usuarioDb.RolId = usuarioInput.RolId;
            usuarioDb.Nombres = usuarioInput.Nombres;
            usuarioDb.Apellidos = usuarioInput.Apellidos;
            usuarioDb.Dni = usuarioInput.Dni;
            usuarioDb.Email = usuarioInput.Email;
            usuarioDb.Telefono = usuarioInput.Telefono;
            usuarioDb.FechaNacimiento = usuarioInput.FechaNacimiento;
            usuarioDb.Estado = usuarioInput.Estado;

            if (!string.IsNullOrWhiteSpace(nuevaPassword)) {
                var passwordHasher = new PasswordHasher<Usuario>();

                usuarioDb.PasswordHash = passwordHasher.HashPassword(usuarioDb, nuevaPassword);
            }

            var coachAsociado = await _context.Coaches
                .FirstOrDefaultAsync(c => c.UsuarioId == usuarioDb.UsuarioId);

            if (coachAsociado != null) {
                coachAsociado.Activo =
                    usuarioDb.Estado == EstadoUsuario.Activo;
            }
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id) {
            var usuario = await _context.Usuarios.FindAsync(id);

            if (usuario == null) {
                return false;
            }

            // Soft delete: marcar como Inactivo en lugar de eliminar
            usuario.Estado = EstadoUsuario.Inactivo;

            // Desactivar coach asociado si existe
            var coach = await _context.Coaches.FirstOrDefaultAsync(c => c.UsuarioId == id);
            if (coach != null)
                coach.Activo = false;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsAsync(int id) {
            return await _context.Usuarios
                .AnyAsync(u => u.UsuarioId == id);
        }

        public async Task<SelectList> GetRolesSelectListAsync(int? selected = null) {
            var roles = await _context.Roles.ToListAsync();

            return new SelectList(
                roles,
                "RolId",
                "Nombre",
                selected
                );
        }

        private string GenerarCodigoMembresia() {
            return $"GM-{Guid.NewGuid().ToString()[..8].ToUpper()}";
        }
    }
}
