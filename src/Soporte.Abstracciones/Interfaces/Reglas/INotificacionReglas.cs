using Soporte.Abstracciones.DTOs;
using Soporte.Abstracciones.Modelos;

namespace Soporte.Abstracciones.Interfaces.Reglas;

/// <summary>Decide quién recibe una notificación y con qué mensaje. Reglas puras (sin E/S).</summary>
public interface INotificacionReglas
{
    /// <summary>
    /// Cambio de estado. Si el equipo lo hace, se notifica al cliente; si el cliente reabre,
    /// al agente asignado. Nunca se notifica al propio autor.
    /// </summary>
    /// <param name="ticket">Ticket ya actualizado.</param>
    /// <param name="estadoAnteriorId">Estado antes del cambio.</param>
    /// <param name="estadoNuevoId">Estado después del cambio.</param>
    /// <param name="historialId">Registro de HistorialTicket del cambio; identifica el evento.</param>
    /// <param name="autor">Usuario que hizo el cambio.</param>
    /// <param name="conMensajePublico">El cambio incluye una explicación pública: se emite una sola notificación.</param>
    IReadOnlyList<NotificacionPendienteDto> PorCambioEstado(
        Ticket ticket, int estadoAnteriorId, int estadoNuevoId, int historialId, UsuarioContextoDto autor, bool conMensajePublico);

    /// <summary>Comentario nuevo. Las notas internas no notifican a nadie.</summary>
    IReadOnlyList<NotificacionPendienteDto> PorComentario(Ticket ticket, Comentario comentario, UsuarioContextoDto autor);
}
