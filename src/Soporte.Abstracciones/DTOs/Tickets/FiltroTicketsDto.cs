using System.ComponentModel.DataAnnotations;

namespace Soporte.Abstracciones.DTOs;

/// <summary>HU-05: filtros por estado, prioridad, agente y búsqueda por asunto o nombre de cliente.</summary>
public class FiltroTicketsDto
{
    public int? EstadoId { get; set; }
    public int? PrioridadId { get; set; }
    public int? AgenteId { get; set; }
    public bool SoloSinAsignar { get; set; }

    /// <summary>Solo abiertos o en progreso que están sin asignar o con el SLA vencido (Dashboard).</summary>
    public bool RequiereAtencion { get; set; }

    /// <summary>Ordena por la última actividad (actualización o creación) en lugar de la fecha de creación.</summary>
    public bool OrdenarPorActividad { get; set; }

    [StringLength(100)]
    public string? Texto { get; set; }

    [Range(1, int.MaxValue)]
    public int Pagina { get; set; } = 1;

    [Range(1, 100)]
    public int TamanoPagina { get; set; } = 20;
}
