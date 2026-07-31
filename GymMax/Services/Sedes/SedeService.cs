using GymMax.Data;
using GymMax.Models;
using Microsoft.EntityFrameworkCore;

namespace GymMax.Services.Sedes {
    public class SedeService : ISedeService {

        private readonly AppDbContext _context;

        public SedeService(AppDbContext context) {
            _context = context;
        }

        public async Task<List<Sede>> ObtenerTodasAsync(string? nombre, bool? activo) {
            var query = _context.Sedes.AsQueryable();

            if (!string.IsNullOrWhiteSpace(nombre)) {
                query = query.Where(s => s.Nombre.Contains(nombre));
            }

            if (activo.HasValue) {
                query = query.Where(s => s.Activo == activo.Value);
            }

            return await query.ToListAsync();
        }

        public async Task<Sede?> ObtenerPorIdAsync(int id) {
            return await _context.Sedes
                .FirstOrDefaultAsync(s => s.SedeId == id);
        }

        public async Task CrearAsync(Sede sede) {
            _context.Sedes.Add(sede);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> ActualizarAsync(Sede sede) {
            var sedeExistente = await _context.Sedes
                .FirstOrDefaultAsync(s => s.SedeId == sede.SedeId);

            if (sedeExistente == null) {
                return false;
            }

            sedeExistente.Nombre = sede.Nombre;
            sedeExistente.Direccion = sede.Direccion;
            sedeExistente.Telefono = sede.Telefono;
            sedeExistente.Horario = sede.Horario;
            sedeExistente.Activo = sede.Activo;

            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> EliminarAsync(int id) {
            var sede = await _context.Sedes
                .FirstOrDefaultAsync(s => s.SedeId == id);

            if (sede == null) {
                return false;
            }

            _context.Sedes.Remove(sede);

            await _context.SaveChangesAsync();

            return true;
        }
    }
}
