using GymMax.ViewModels;

namespace GymMax.Services.Dashboard {
    public interface IDashboardService {
        Task<DashboardViewModel> ObtenerDashboardAsync();
    }
}
