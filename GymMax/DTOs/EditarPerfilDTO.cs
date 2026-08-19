using System.ComponentModel.DataAnnotations;

namespace GymMax.DTOs
{
    public class EditarPerfilDTO
    {
        [Required(ErrorMessage = "El email es obligatorio.")]
        [EmailAddress(ErrorMessage = "El formato del email no es válido.")]
        [MaxLength(150)]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "El teléfono es obligatorio.")]
        [RegularExpression(@"^\d{9}$", ErrorMessage = "El teléfono debe tener exactamente 9 dígitos numéricos.")]
        public string Telefono { get; set; } = string.Empty;
    }
}
