using GymMax.Data;
using GymMax.Domain.Entities;
using GymMax.Enums;
using GymMax.Models;
using Microsoft.EntityFrameworkCore;

namespace GymMax.Services.Suscripciones {
    public class SuscripcionService : ISuscripcionService {

        private readonly AppDbContext _context;

        public SuscripcionService(AppDbContext context) {
            _context = context;
        }

        public async Task<List<Suscripcion>> ObtenerTodasAsync() {
            return await _context.Suscripciones
                .Include(s => s.Usuario)
                .Include(s => s.Plan)
                .ToListAsync();
        }

        public async Task<Suscripcion?> ObtenerPorIdAsync(int id) {
            return await _context.Suscripciones
                .Include(s => s.Usuario)
                .Include(s => s.Plan)
                .FirstOrDefaultAsync(s => s.SuscripcionId == id);
        }

        public async Task<List<Usuario>> ObtenerClientesActivosAsync() {
            return await _context.Usuarios
                .Where(u =>
                    u.RolId == (int)RolUsuario.Cliente &&
                    u.Estado == EstadoUsuario.Activo)
                .ToListAsync();
        }

        public async Task<List<Plan>> ObtenerPlanesActivosAsync() {
            return await _context.Planes
                .Where(p => p.Activo)
                .ToListAsync();
        }

        public async Task CrearAsync(Suscripcion suscripcion) {
            _context.Suscripciones.Add(suscripcion);

            await _context.SaveChangesAsync();
        }

        public async Task<bool> ActualizarAsync(Suscripcion suscripcion) {
            var suscripcionDb = await _context.Suscripciones
                .FindAsync(suscripcion.SuscripcionId);

            if (suscripcionDb == null) {
                return false;
            }

            suscripcionDb.UsuarioId = suscripcion.UsuarioId;
            suscripcionDb.PlanId = suscripcion.PlanId;
            suscripcionDb.PrecioPactado = suscripcion.PrecioPactado;
            suscripcionDb.FechaInicio = suscripcion.FechaInicio;
            suscripcionDb.FechaFin = suscripcion.FechaFin;
            suscripcionDb.Estado = suscripcion.Estado;

            await _context.SaveChangesAsync();

            return true;
        }

        public async Task EliminarAsync(int id) {
            var suscripcion = await _context.Suscripciones
                .FindAsync(id);

            if (suscripcion == null) {
                return;
            }

            _context.Suscripciones.Remove(suscripcion);

            await _context.SaveChangesAsync();
        }

        public async Task<bool> ExisteAsync(int id) {
            return await _context.Suscripciones
                .AnyAsync(s => s.SuscripcionId == id);
        }
    }
}
