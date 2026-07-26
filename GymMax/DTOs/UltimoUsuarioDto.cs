namespace GymMax.DTOs {
    public class UltimoUsuarioDto {
        public string NombreCompleto { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Rol { get; set; } = string.Empty;
        public DateTime FechaRegistro { get; set; }
    }
}
