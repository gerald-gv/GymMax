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
            var hoy = DateOnly.FromDateTime(DateTime.Today);

            // Auto-vencer suscripciones cuya FechaFin ya pasó y siguen Activas
            var vencidas = await _context.Suscripciones
                .Where(s => s.Estado == EstadoSuscripcion.Activa && s.FechaFin < hoy)
                .ToListAsync();

            if (vencidas.Any()) {
                vencidas.ForEach(s => s.Estado = EstadoSuscripcion.Vencida);
                await _context.SaveChangesAsync();
            }

            return await _context.Suscripciones
                .Include(s => s.Usuario)
                .Include(s => s.Plan)
                .ToListAsync();
        }

        public async Task<Suscripcion?> ObtenerPorIdAsync(int id) {
            var suscripcion = await _context.Suscripciones
                .Include(s => s.Usuario)
                .Include(s => s.Plan)
                .FirstOrDefaultAsync(s => s.SuscripcionId == id);

            // Auto-vencer si aplica
            if (suscripcion != null
                && suscripcion.Estado == EstadoSuscripcion.Activa
                && suscripcion.FechaFin < DateOnly.FromDateTime(DateTime.Today)) {
                suscripcion.Estado = EstadoSuscripcion.Vencida;
                await _context.SaveChangesAsync();
            }

            return suscripcion;
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

        public async Task CrearAsync(Suscripcion suscripcion)
        {
            var plan = await _context.Planes.AsNoTracking()
                .FirstOrDefaultAsync(p => p.PlanId == suscripcion.PlanId);
            if (plan == null)
                throw new InvalidOperationException("El plan seleccionado no existe.");
            suscripcion.FechaInicio = DateOnly.FromDateTime(DateTime.Now); // fija, no negociable

            _context.Suscripciones.Add(suscripcion);
            await _context.SaveChangesAsync(); // Necesario para que EF genere el SuscripcionId

            var pago = new Pago
            {
                SuscripcionId = suscripcion.SuscripcionId,
                Monto = suscripcion.PrecioPactado,
                FechaPago = DateTime.Now
            };

            _context.Pagos.Add(pago);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> ActualizarAsync(Suscripcion suscripcion) {
            var suscripcionDb = await _context.Suscripciones
                .FindAsync(suscripcion.SuscripcionId);

            if (suscripcionDb == null) {
                return false;
            }

            // Una suscripción cancelada no puede reactivarse
            if (suscripcionDb.Estado == EstadoSuscripcion.Cancelada
                && suscripcion.Estado == EstadoSuscripcion.Activa) {
                return false;
            }

            suscripcionDb.UsuarioId     = suscripcion.UsuarioId;
            suscripcionDb.PlanId        = suscripcion.PlanId;
            suscripcionDb.PrecioPactado = suscripcion.PrecioPactado;
            suscripcionDb.FechaInicio   = suscripcion.FechaInicio;
            suscripcionDb.FechaFin      = suscripcion.FechaFin;
            suscripcionDb.Estado        = suscripcion.Estado;

            await _context.SaveChangesAsync();

            return true;
        }

        public async Task EliminarAsync(int id) {
            var suscripcion = await _context.Suscripciones.FindAsync(id);
            if (suscripcion == null) return;
            _context.Suscripciones.Remove(suscripcion);
            await _context.SaveChangesAsync();
        }

        public async Task CancelarAsync(int id) {
            var suscripcion = await _context.Suscripciones.FindAsync(id);
            if (suscripcion == null) return;
            // Soft delete: marcar como Cancelada en lugar de eliminar
            suscripcion.Estado = EstadoSuscripcion.Cancelada;
            await _context.SaveChangesAsync();
        }

        public async Task<bool> ExisteAsync(int id) {
            return await _context.Suscripciones
                .AnyAsync(s => s.SuscripcionId == id);
        }
    }
}
