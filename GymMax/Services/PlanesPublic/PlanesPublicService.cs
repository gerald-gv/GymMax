using GymMax.Data;
using GymMax.Models;
using Microsoft.EntityFrameworkCore;

namespace GymMax.Services.PlanesPublic {
    public class PlanesPublicService : IPlanesPublicService
    {
        private readonly AppDbContext _context;

        public PlanesPublicService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Plan>> ObtenerPlanesActivosAsync()
        {
            return await _context.Planes
                .Where(p => p.Activo)
                .OrderBy(p => p.Precio)
                .ToListAsync();
        }
    }
}
