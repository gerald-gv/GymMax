using GymMax.Domain.Entities;

namespace GymMax.Models
{
    public class ConversacionMiembro
    {
        public int ConversacionMiembroId { get; set; }
        public int ConversacionId { get; set; }
        public int UsuarioId { get; set; }
        public DateTime FechaIngreso { get; set; }
        public bool Activo { get; set; }
        // Relaciones
        public Conversacion Conversacion { get; set; } = null!;
        public Usuario Usuario { get; set; } = null!;
    }
}
