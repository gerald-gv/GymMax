using GymMax.Data;
using GymMax.Enums;
using GymMax.Models;
using Microsoft.EntityFrameworkCore;

namespace GymMax.Services.Coaches {
    public class CoachService : ICoachService {

        private readonly AppDbContext _context;

        public CoachService(AppDbContext context) {
            _context = context;
        }

        public async Task<List<Coach>> ObtenerTodosAsync() {
            return await _context.Coaches
                .Include(c => c.Usuario)
                .Include(c => c.Sede)
                .ToListAsync();
        }

        public async Task<Coach?> ObtenerPorIdAsync(int id) {
            return await _context.Coaches
                .Include(c => c.Usuario)
                .Include(c => c.Sede)
                .FirstOrDefaultAsync(c => c.CoachId == id);
        }

        public async Task<Coach?> ObtenerParaEditarAsync(int id) {
            return await _context.Coaches
                .Include(c => c.Usuario)
                .FirstOrDefaultAsync(c => c.CoachId == id);
        }

        public async Task<List<Sede>> ObtenerSedesAsync() {
            return await _context.Sedes.ToListAsync();
        }

        public async Task CrearAsync(Coach coach) {
            _context.Coaches.Add(coach);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> ActualizarAsync(Coach coach) {
            var coachDb = await _context.Coaches.FindAsync(coach.CoachId);

            if (coachDb == null) {
                return false;
            }

            coachDb.SedeId = coach.SedeId;
            coachDb.FechaIngreso = coach.FechaIngreso;
            coachDb.Activo = coach.Activo;

            var usuarioAsociado = await _context.Usuarios.FindAsync(coachDb.UsuarioId);

            if (usuarioAsociado != null) {
                usuarioAsociado.Estado = coach.Activo ? EstadoUsuario.Activo : EstadoUsuario.Inactivo;
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task EliminarAsync(int id) {
            var coach = await _context.Coaches.FindAsync(id);

            if (coach == null) {
                return;
            }

            _context.Coaches.Remove(coach);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> ExisteAsync(int id) {
            return await _context.Coaches .AnyAsync(c => c.CoachId == id);
        }

    }
}
