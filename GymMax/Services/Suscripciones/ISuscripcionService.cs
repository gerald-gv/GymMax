using GymMax.Domain.Entities;
using GymMax.Models;

namespace GymMax.Services.Suscripciones {
    public interface ISuscripcionService {
        Task<List<Suscripcion>> ObtenerTodasAsync();

        Task<Suscripcion?> ObtenerPorIdAsync(int id);

        Task<List<Usuario>> ObtenerClientesActivosAsync();

        Task<List<Plan>> ObtenerPlanesActivosAsync();

        Task CrearAsync(Suscripcion suscripcion);

        Task<bool> ActualizarAsync(Suscripcion suscripcion);

        Task EliminarAsync(int id);

        Task<bool> ExisteAsync(int id);
    }
}
