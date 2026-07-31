using GymMax.Models;

namespace GymMax.Services.Planes {
    public interface IPlanesService {

        Task<List<Plan>> ObtenerTodosAsync(
            string? nombre,
            decimal? precioMin,
            decimal? precioMax
            );

        Task<Plan?> ObtenerPorIdAsync(int id);

        Task CrearAsync(Plan plan);

        Task ActualizarAsync(Plan plan);

        Task EliminarAsync(int id);

        Task<bool> ExisteAsync(int id);

    }
}
