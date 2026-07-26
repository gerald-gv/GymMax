using GymMax.Domain.Entities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GymMax.Models {
    public class Coach
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int CoachId { get; set; }

        [Required]
        public int UsuarioId { get; set; }

        public int? SedeId { get; set; }

        [Required]
        public DateOnly FechaIngreso { get; set; }

        public bool Activo { get; set; } = true;

        [ForeignKey(nameof(UsuarioId))]
        public Usuario Usuario { get; set; } = null!;

        [ForeignKey(nameof(SedeId))]
        public Sede? Sede { get; set; }
    }
}
