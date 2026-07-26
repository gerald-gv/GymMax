using GymMax.Enums;
using GymMax.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GymMax.Domain.Entities {

    public class Usuario {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int UsuarioId { get; set; }

        [Required]
        public int RolId { get; set; }

        [Required]
        [MaxLength(100)]
        public string Nombres { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string Apellidos { get; set; } = string.Empty;

        [Required]
        [MaxLength(8)]
        public string Dni { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [MaxLength(150)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [Phone]
        [MaxLength(20)]
        public string Telefono { get; set; } = string.Empty;

        [Required]
        public DateOnly FechaNacimiento { get; set; }

        [Required]
        [MaxLength(255)]
        public string PasswordHash { get; set; } = string.Empty;

        [MaxLength(20)]
        public string? CodigoMembresia { get; set; }

        [Required]
        public DateTime FechaRegistro { get; set; }

        [Required]
        public EstadoUsuario Estado { get; set; }

        [ForeignKey(nameof(RolId))]
        public Rol? Rol { get; set; }
    }
}