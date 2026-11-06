using System.ComponentModel.DataAnnotations;

namespace Soporte.Abstracciones.DTOs;

public class CrearPlantillaDto
{
    [Required, MaxLength(100)]
    public string Titulo { get; set; } = string.Empty;

    [Required, MaxLength(4000)]
    public string Texto { get; set; } = string.Empty;
}
