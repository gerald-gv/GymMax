namespace GymMax.Models
{
    public class DashboardViewModel
    {
        // Contadores principales
        public int TotalUsuarios       { get; set; }
        public int TotalClientes       { get; set; }
        public int TotalCoaches        { get; set; }
        public int TotalPlanes         { get; set; }
        public int TotalSedes          { get; set; }
        public int TotalSuscripciones  { get; set; }

        // Suscripciones activas e ingresos del mes
        public int     SuscripcionesActivas { get; set; }
        public decimal IngresosMes          { get; set; }

        // Asistencias de hoy
        public int AsistenciasHoy { get; set; }

        // Últimos 5 usuarios registrados
        public List<UltimoUsuarioDto> UltimosUsuarios { get; set; } = new();
    }

    public class UltimoUsuarioDto
    {
        public string NombreCompleto { get; set; } = string.Empty;
        public string Email          { get; set; } = string.Empty;
        public string Rol            { get; set; } = string.Empty;
        public DateTime FechaRegistro { get; set; }
    }
}
