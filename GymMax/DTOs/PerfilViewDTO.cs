namespace GymMax.DTOs
{
    public class PerfilViewDTO
    {
        public int UsuarioId { get; set; }
        public string Nombres { get; set; } = string.Empty;
        public string Apellidos { get; set; } = string.Empty;
        public string Dni { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public DateOnly FechaNacimiento { get; set; }
        public string? CodigoMembresia { get; set; }
        public DateTime FechaRegistro { get; set; }
        public string Estado { get; set; } = string.Empty;
    }
}
