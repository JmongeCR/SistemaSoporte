using Soporte.Abstracciones.Enumeraciones;

namespace Soporte.Abstracciones.DTOs;

public class NotificacionDto
{
    public int Id { get; init; }
    public int TicketId { get; init; }
    public string TicketAsunto { get; init; } = string.Empty;
    public TipoNotificacion Tipo { get; init; }
    public string Mensaje { get; init; } = string.Empty;

    /// <summary>UTC.</summary>
    public DateTime FechaCreacion { get; init; }
    public bool Leida { get; init; }
}
