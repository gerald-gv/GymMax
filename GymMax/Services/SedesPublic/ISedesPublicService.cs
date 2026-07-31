using GymMax.Models;

namespace GymMax.Services.SedesPublic {
    public interface ISedesPublicService {
        Task<List<Sede>> ObtenerSedesActivasAsync();
    }
}
