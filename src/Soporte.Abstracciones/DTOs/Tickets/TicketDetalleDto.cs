namespace Soporte.Abstracciones.DTOs;

public class TicketDetalleDto : TicketResumenDto
{
    public string Descripcion { get; init; } = string.Empty;
    public int CategoriaId { get; init; }
    public int ClienteId { get; init; }
    public int? AgenteAsignadoId { get; init; }
    public string RowVersion { get; init; } = string.Empty;

    public IReadOnlyList<AdjuntoDto> Adjuntos { get; init; } = Array.Empty<AdjuntoDto>();
    public IReadOnlyList<ComentarioDto> Comentarios { get; init; } = Array.Empty<ComentarioDto>();
    public IReadOnlyList<HistorialTicketDto> Historial { get; init; } = Array.Empty<HistorialTicketDto>();
}
