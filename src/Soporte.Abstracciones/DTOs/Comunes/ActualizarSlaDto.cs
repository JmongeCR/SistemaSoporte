using System.ComponentModel.DataAnnotations;

namespace Soporte.Abstracciones.DTOs;

public class ActualizarSlaDto
{
    [Range(1, 720, ErrorMessage = "Las horas de SLA deben ser entre 1 y 720.")]
    public int HorasSla { get; init; }
}
