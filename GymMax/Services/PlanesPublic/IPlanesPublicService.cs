using GymMax.Models;

namespace GymMax.Services.PlanesPublic {
    public interface IPlanesPublicService {
        Task<List<Plan>> ObtenerPlanesActivosAsync();
    }
}
