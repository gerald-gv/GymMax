using GymMax.Domain.Entities;

namespace GymMax.Models
{
    public class Mensaje
    {
        public int MensajeId { get; set; }
        public int ConversacionId { get; set; }
        public int UsuarioId { get; set; }
        public string Contenido { get; set; } = string.Empty;
        public DateTime FechaEnvio { get; set; }
        public bool Editado { get; set; }
        public bool Eliminado { get; set; }
        // Relaciones
        public Conversacion Conversacion { get; set; } = null!;
        public Usuario Usuario { get; set; } = null!;
    }
}
