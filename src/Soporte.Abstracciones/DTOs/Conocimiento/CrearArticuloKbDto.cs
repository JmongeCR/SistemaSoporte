using System.ComponentModel.DataAnnotations;

namespace Soporte.Abstracciones.DTOs;

public class CrearArticuloKbDto
{
    [Required, MaxLength(200)]
    public string Titulo    { get; set; } = string.Empty;

    [Required]
    public string Contenido { get; set; } = string.Empty;

    [Range(1, int.MaxValue)]
    public int CategoriaId  { get; set; }

    public bool Publicado   { get; set; } = true;
}
