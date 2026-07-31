using GymMax.Data;
using GymMax.DTOs;
using GymMax.Enums;
using GymMax.Models;
using Microsoft.EntityFrameworkCore;

namespace GymMax.Services.Dashboard {
    public class DashboardService : IDashboardService {

        private readonly AppDbContext _context;

        public DashboardService(AppDbContext context) {
            _context = context;
        }

        public async Task<DashboardViewModel> ObtenerDashboardAsync() {
            var hoy = DateTime.Today;
            var inicioMes = new DateTime(hoy.Year, hoy.Month, 1);

            return new DashboardViewModel {
                TotalUsuarios = await _context.Usuarios.CountAsync(),

                TotalClientes = await _context.Usuarios
                    .CountAsync(u => u.RolId == (int)RolUsuario.Cliente),

                TotalCoaches = await _context.Usuarios
                    .CountAsync(u => u.RolId == (int)RolUsuario.Coach),

                TotalPlanes = await _context.Planes.CountAsync(),

                TotalSedes = await _context.Sedes.CountAsync(),

                TotalSuscripciones = await _context.Suscripciones.CountAsync(),

                SuscripcionesActivas = await _context.Suscripciones
                    .CountAsync(s => s.Estado == EstadoSuscripcion.Activa),

                IngresosMes = await _context.Pagos
                    .Where(p => p.FechaPago >= inicioMes)
                    .SumAsync(p => (decimal?)p.Monto) ?? 0,

                AsistenciasHoy = await _context.Asistencias
                    .CountAsync(a => a.FechaHoraEntrada.Date == hoy),

                UltimosUsuarios = await _context.Usuarios
                    .Include(u => u.Rol)
                    .OrderByDescending(u => u.FechaRegistro)
                    .Take(5)
                    .Select(u => new UltimoUsuarioDto {
                        NombreCompleto = u.Nombres + " " + u.Apellidos,
                        Email = u.Email,
                        Rol = u.Rol!.Nombre,
                        FechaRegistro = u.FechaRegistro
                    })
                    .ToListAsync()
            };
        }
    }
}
