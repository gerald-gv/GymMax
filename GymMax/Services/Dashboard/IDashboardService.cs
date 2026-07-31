using GymMax.Models;

namespace GymMax.Services.Dashboard {
    public interface IDashboardService {
        Task<DashboardViewModel> ObtenerDashboardAsync();
    }
}
