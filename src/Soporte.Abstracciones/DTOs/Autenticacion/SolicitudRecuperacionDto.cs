using System.ComponentModel.DataAnnotations;

namespace Soporte.Abstracciones.DTOs;

public class SolicitudRecuperacionDto
{
    [Required(ErrorMessage = "El correo es obligatorio.")]
    [EmailAddress(ErrorMessage = "El correo no tiene un formato válido.")]
    public string Correo { get; set; } = string.Empty;
}
