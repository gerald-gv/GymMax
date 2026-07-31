using GymMax.Models;

namespace GymMax.Services.Sedes {
    public interface ISedeService {
        Task<List<Sede>> ObtenerTodasAsync(string? nombre, bool? activo);
        Task<Sede?> ObtenerPorIdAsync(int id);
        Task CrearAsync(Sede sede);
        Task<bool> ActualizarAsync(Sede sede);
        Task<bool> EliminarAsync(int id);
    }
}
