using GymMax.Domain.Entities;
using GymMax.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GymMax.Models {
    public class Suscripcion {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int SuscripcionId { get; set; }

        [Required]
        public int UsuarioId { get; set; }

        [Required]
        public int PlanId { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal PrecioPactado { get; set; }

        [Required]
        public DateOnly FechaInicio { get; set; }

        [Required]
        public DateOnly FechaFin { get; set; }

        [Required]
        public EstadoSuscripcion Estado { get; set; }

        [ForeignKey(nameof(UsuarioId))]
        public Usuario Usuario { get; set; } = null!;

        [ForeignKey(nameof(PlanId))]
        public Plan Plan { get; set; } = null!;

    }
}
