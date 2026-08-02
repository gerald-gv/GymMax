using GymMax.Data;
using GymMax.Models;
using Microsoft.EntityFrameworkCore;

namespace GymMax.Services.Planes {
    public class PlanesService : IPlanesService {

        private readonly AppDbContext _context;

        public PlanesService(AppDbContext context) {
            _context = context;
        }

        public async Task<List<Plan>> ObtenerTodosAsync(
            string? nombre,
            decimal? precioMin,
            decimal? precioMax
            ) {
            var query = _context.Planes.AsQueryable();

            if (!string.IsNullOrWhiteSpace(nombre))
                query = query.Where(p => p.Nombre.Contains(nombre));

            if (precioMin.HasValue)
                query = query.Where(p => p.Precio >= precioMin.Value);

            if (precioMax.HasValue)
                query = query.Where(p => p.Precio <= precioMax.Value);

            return await query.ToListAsync();
        }

        public async Task<Plan?> ObtenerPorIdAsync(int id) {
            return await _context.Planes
                .FirstOrDefaultAsync(x => x.PlanId == id);
        }

        public async Task CrearAsync(Plan plan) {
            _context.Planes.Add(plan);
            await _context.SaveChangesAsync();
        }

        public async Task ActualizarAsync(Plan plan) {
            _context.Planes.Update(plan);
            await _context.SaveChangesAsync();
        }

        public async Task EliminarAsync(int id) {
            var plan = await _context.Planes.FindAsync(id);

            if (plan is null) return;

            // Soft delete: desactivar en lugar de eliminar
            plan.Activo = false;
            await _context.SaveChangesAsync();
        }

        public async Task<bool> ExisteAsync(int id) {
            return await _context.Planes
                .AnyAsync(x => x.PlanId == id);
        }
    }
}
