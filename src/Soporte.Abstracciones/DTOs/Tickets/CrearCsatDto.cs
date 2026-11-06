using System.ComponentModel.DataAnnotations;

namespace Soporte.Abstracciones.DTOs;

public class CrearCsatDto
{
    [Range(1, 5)]
    public int Calificacion { get; set; }

    [MaxLength(500)]
    public string? Comentario { get; set; }
}
