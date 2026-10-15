using System.ComponentModel.DataAnnotations;

namespace Soporte.Abstracciones.Modelos;

/// <summary>Catálogo de prioridades. HorasSla define el plazo para la métrica "SLA vencido" (HU-12).</summary>
public class Prioridad
{
    public int Id { get; set; }

    [Required, StringLength(20)]
    public string Nombre { get; set; } = string.Empty;

    [Range(1, 720)]
    public int HorasSla { get; set; }
}
