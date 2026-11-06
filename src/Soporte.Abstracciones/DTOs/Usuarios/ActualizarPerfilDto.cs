using System.ComponentModel.DataAnnotations;

namespace Soporte.Abstracciones.DTOs;

public class ActualizarPerfilDto
{
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [MaxLength(100, ErrorMessage = "El nombre no puede superar los 100 caracteres.")]
    public string Nombre { get; init; } = string.Empty;

    public string? ContrasenaActual { get; init; }

    [MinLength(8, ErrorMessage = "La nueva contraseña debe tener al menos 8 caracteres.")]
    [MaxLength(100, ErrorMessage = "La contraseña no puede superar los 100 caracteres.")]
    public string? NuevaContrasena { get; init; }
}
