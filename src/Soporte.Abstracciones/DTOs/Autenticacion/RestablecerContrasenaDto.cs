using System.ComponentModel.DataAnnotations;

namespace Soporte.Abstracciones.DTOs;

public class RestablecerContrasenaDto
{
    [Required]
    public string Token { get; set; } = string.Empty;

    [Required(ErrorMessage = "La nueva contraseña es obligatoria.")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "La contraseña debe tener al menos 8 caracteres.")]
    [DataType(DataType.Password)]
    public string NuevaContrasena { get; set; } = string.Empty;

    [Compare(nameof(NuevaContrasena), ErrorMessage = "Las contraseñas no coinciden.")]
    [DataType(DataType.Password)]
    public string ConfirmarContrasena { get; set; } = string.Empty;
}
