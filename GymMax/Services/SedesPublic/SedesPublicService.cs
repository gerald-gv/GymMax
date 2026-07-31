using GymMax.Data;
using GymMax.Models;
using Microsoft.EntityFrameworkCore;

namespace GymMax.Services.SedesPublic {
    public class SedesPublicService : ISedesPublicService {

        private readonly AppDbContext _context;

        public SedesPublicService(AppDbContext context) {
            _context = context;
        }

        public async Task<List<Sede>> ObtenerSedesActivasAsync() {
            return await _context.Sedes
                .Where(s => s.Activo)
                .ToListAsync();
        }

    }
}
