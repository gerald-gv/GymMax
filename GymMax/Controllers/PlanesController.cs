using GymMax.Services.PlanesPublic;
using Microsoft.AspNetCore.Mvc;

namespace GymMax.Controllers {
    public class PlanesController : Controller {

        private readonly IPlanesPublicService _planesService;

        public PlanesController(IPlanesPublicService planesService) {
            _planesService = planesService;
        }

        public async Task<IActionResult> Index() {
            var planes = await _planesService.ObtenerPlanesActivosAsync();
            return View(planes);
        }
    }
}
