using Soporte.Abstracciones.Enumeraciones;

namespace Soporte.Abstracciones.DTOs;

/// <summary>Notificación decidida por las reglas de negocio, pendiente de guardarse.</summary>
public class NotificacionPendienteDto
{
    public int UsuarioId { get; init; }
    public int TicketId { get; init; }
    public TipoNotificacion Tipo { get; init; }
    public string Mensaje { get; init; } = string.Empty;
    public string ClaveEvento { get; init; } = string.Empty;
}
