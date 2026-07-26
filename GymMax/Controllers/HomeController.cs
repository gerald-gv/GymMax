
using GymMax.Data;
using GymMax.Enums;
using GymMax.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace GymMax.Controllers {
    [Authorize(Roles = "Administrador,Coach")]
    public class HomeController : Controller {

        private readonly AppDbContext _context;

        public HomeController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var hoy = DateTime.Today;
            var inicioMes = new DateTime(hoy.Year, hoy.Month, 1);

            var vm = new DashboardViewModel
            {
                TotalUsuarios      = await _context.Usuarios.CountAsync(),
                TotalClientes      = await _context.Usuarios.CountAsync(u => u.RolId == (int)RolUsuario.Cliente),
                TotalCoaches       = await _context.Usuarios.CountAsync(u => u.RolId == (int)RolUsuario.Coach),
                TotalPlanes        = await _context.Planes.CountAsync(),
                TotalSedes         = await _context.Sedes.CountAsync(),
                TotalSuscripciones = await _context.Suscripciones.CountAsync(),

                SuscripcionesActivas = await _context.Suscripciones
                    .CountAsync(s => s.Estado == GymMax.Enums.EstadoSuscripcion.Activa),

                IngresosMes = await _context.Pagos
                    .Where(p => p.FechaPago >= inicioMes)
                    .SumAsync(p => (decimal?)p.Monto) ?? 0,

                AsistenciasHoy = await _context.Asistencias
                    .CountAsync(a => a.FechaHoraEntrada.Date == hoy),

                UltimosUsuarios = await _context.Usuarios
                    .Include(u => u.Rol)
                    .OrderByDescending(u => u.FechaRegistro)
                    .Take(5)
                    .Select(u => new UltimoUsuarioDto
                    {
                        NombreCompleto = u.Nombres + " " + u.Apellidos,
                        Email          = u.Email,
                        Rol            = u.Rol!.Nombre,
                        FechaRegistro  = u.FechaRegistro
                    })
                    .ToListAsync()
            };

            return View(vm);
        }
        
        [AllowAnonymous]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error() {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

    }
}

