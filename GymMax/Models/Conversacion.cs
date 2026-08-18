using GymMax.Domain.Entities;
using GymMax.Enums;

namespace GymMax.Models
{
    public class Conversacion
    {
        public int ConversacionId { get; set; }
        public TipoConversacion Tipo { get; set; }
        public string? Nombre { get; set; }
        public DateTime FechaCreacion { get; set; }
        public int CreadaPorUsuarioId { get; set; }
        public bool Activa { get; set; }
        // Usuario que creó la conversación
        public Usuario CreadaPorUsuario { get; set; } = null!;
        // Miembros de la conversación
        public ICollection<ConversacionMiembro> Miembros { get; set; }
            = new List<ConversacionMiembro>();
        // Mensajes de la conversación
        public ICollection<Mensaje> Mensajes { get; set; }
            = new List<Mensaje>();
    }
}
