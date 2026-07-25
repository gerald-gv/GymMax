using GymMax.Domain.Entities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GymMax.Models {
    public class Asistencia {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int AsistenciaId { get; set; }

        [Required]
        public int UsuarioId { get; set; }

        [Required]
        public int SedeId { get; set; }

        [Required]
        public DateTime FechaHoraEntrada { get; set; }

        [ForeignKey(nameof(UsuarioId))]
        public Usuario Usuario { get; set; } = null!;

        [ForeignKey(nameof(SedeId))]
        public Sede Sede { get; set; } = null!;
    }
}
