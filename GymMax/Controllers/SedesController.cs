using GymMax.Services.SedesPublic;
using Microsoft.AspNetCore.Mvc;

namespace GymMax.Controllers {
    public class SedesController : Controller {

        private readonly ISedesPublicService _sedeService;

        public SedesController(ISedesPublicService sedeService) {
            _sedeService = sedeService;
        }

        public async Task<IActionResult> Index() {
            var sedes = await _sedeService.ObtenerSedesActivasAsync();

            return View(sedes);
        }
    }
}
