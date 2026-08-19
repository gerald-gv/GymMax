using System.ComponentModel.DataAnnotations;

namespace GymMax.DTOs
{
    public class CambiarPasswordDTO
    {
        [Required(ErrorMessage = "Debes ingresar tu contraseña actual.")]
        [DataType(DataType.Password)]
        public string PasswordActual { get; set; } = string.Empty;

        [Required(ErrorMessage = "Debes ingresar la nueva contraseña.")]
        [DataType(DataType.Password)]
        [MinLength(8, ErrorMessage = "La contraseña debe tener al menos 8 caracteres.")]
        public string PasswordNueva { get; set; } = string.Empty;

        [Required(ErrorMessage = "Debes confirmar la nueva contraseña.")]
        [DataType(DataType.Password)]
        [Compare(nameof(PasswordNueva), ErrorMessage = "Las contraseñas no coinciden.")]
        public string ConfirmarPasswordNueva { get; set; } = string.Empty;
    }
}
