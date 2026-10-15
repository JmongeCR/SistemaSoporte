namespace Soporte.Abstracciones.DTOs;

/// <summary>Fila de la lista de tickets (HU-05). Las fechas están en UTC.</summary>
public class TicketResumenDto
{
    public int Id { get; init; }
    public string Asunto { get; init; } = string.Empty;
    public string Categoria { get; init; } = string.Empty;
    public int PrioridadId { get; init; }
    public string Prioridad { get; init; } = string.Empty;
    public int EstadoId { get; init; }
    public string Estado { get; init; } = string.Empty;
    public string Cliente { get; init; } = string.Empty;
    public string? AgenteAsignado { get; init; }
    public DateTime FechaCreacion { get; init; }
    public DateTime? FechaActualizacion { get; init; }
    public bool SlaVencido { get; init; }

    /// <summary>El último mensaje público es una solicitud de información del equipo sin respuesta del cliente.</summary>
    public bool EsperaRespuestaCliente { get; init; }
}
