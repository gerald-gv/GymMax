using GymMax.Models;

namespace GymMax.Services.Coaches {
    public interface ICoachService {

        Task<List<Coach>> ObtenerTodosAsync();

        Task<Coach?> ObtenerPorIdAsync(int id);

        Task<Coach?> ObtenerParaEditarAsync(int id);

        Task<List<Sede>> ObtenerSedesAsync();

        Task CrearAsync(Coach coach);

        Task<bool> ActualizarAsync(Coach coach);

        Task EliminarAsync(int id);

        Task<bool> ExisteAsync(int id);

    }
}
