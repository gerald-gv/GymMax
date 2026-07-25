using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GymMax.Models {
    public class Sede {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int SedeId { get; set; }

        [Required]
        [MaxLength(100)]
        public string Nombre { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string Direccion { get; set; } = string.Empty;

        [Required]
        [Phone]
        [MaxLength(20)]
        public string Telefono { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        public string Horario { get; set; } = string.Empty;

        public bool Activo { get; set; } = true;

    }
}
