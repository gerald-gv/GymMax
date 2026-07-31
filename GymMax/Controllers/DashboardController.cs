using GymMax.Enums;
using GymMax.Services.Dashboard;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GymMax.Controllers {

    [Authorize(Roles = nameof(RolUsuario.Administrador))]
    public class DashboardController : Controller
    {
        private readonly IDashboardService _dashboardService;

        public DashboardController(IDashboardService dashboardService) {
            _dashboardService = dashboardService;
        }

        public async Task<IActionResult> Index() {
            var vm = await _dashboardService.ObtenerDashboardAsync();

            return View(vm);
        }
    }
}
