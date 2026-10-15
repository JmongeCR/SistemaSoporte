using Soporte.Abstracciones.DTOs;

namespace Soporte.Web.Presentacion;

/// <summary>Elemento de la línea de tiempo del ticket: un cambio de estado o un comentario.</summary>
public class ItemActividad
{
    public DateTime Fecha { get; init; }
    public HistorialTicketDto? Evento { get; init; }
    public ComentarioDto? Comentario { get; init; }

    /// <summary>Une historial y comentarios en orden cronológico (a igual fecha, el cambio de estado va primero).</summary>
    public static IReadOnlyList<ItemActividad> Combinar(TicketDetalleDto ticket) =>
        ticket.Historial.Select(h => new ItemActividad { Fecha = h.FechaCambio, Evento = h })
            .Concat(ticket.Comentarios.Select(c => new ItemActividad { Fecha = c.FechaCreacion, Comentario = c }))
            .OrderBy(i => i.Fecha)
            .ThenBy(i => i.Evento is null ? 1 : 0)
            .ToList();
}
