using Soporte.Abstracciones.DTOs;
using Soporte.Abstracciones.Enumeraciones;
using Soporte.Abstracciones.Interfaces.Reglas;
using Soporte.Abstracciones.Modelos;

namespace Soporte.Reglas.Reglas;

/// <summary>
/// Quién se entera de qué:
/// - El equipo cambia el estado o responde en público → el cliente dueño del ticket.
/// - El cliente responde o reabre → el agente asignado (si hay).
/// - Notas internas, cambios de prioridad/categoría/agente y acciones propias → nadie.
/// </summary>
public class NotificacionReglas : INotificacionReglas
{
    private static readonly IReadOnlyList<NotificacionPendienteDto> Ninguna = Array.Empty<NotificacionPendienteDto>();

    public IReadOnlyList<NotificacionPendienteDto> PorCambioEstado(
        Ticket ticket, int estadoAnteriorId, int estadoNuevoId, int historialId, UsuarioContextoDto autor, bool conMensajePublico)
    {
        if (estadoAnteriorId == estadoNuevoId)
            return Ninguna;

        var clave = $"historial:{historialId}";
        var anterior = (EstadoTicketTipo)estadoAnteriorId;
        var nuevo = (EstadoTicketTipo)estadoNuevoId;

        if (autor.Rol == RolTipo.Cliente)
        {
            // El cliente solo puede reabrir (HU-08 esc. 2): se avisa al agente que lo atiende.
            return ticket.AgenteAsignadoId is int agenteId && agenteId != autor.UsuarioId
                ? Una(agenteId, ticket.Id, TipoNotificacion.Reabierto, $"El cliente reabrió el ticket #{ticket.Id}", clave)
                : Ninguna;
        }

        if (ticket.ClienteId == autor.UsuarioId)
            return Ninguna;

        var (tipo, mensaje) = nuevo switch
        {
            EstadoTicketTipo.Resuelto => (TipoNotificacion.Resuelto,
                conMensajePublico
                    ? $"Tu ticket #{ticket.Id} fue resuelto. Revisá la explicación del equipo."
                    : $"Tu ticket #{ticket.Id} fue resuelto"),
            EstadoTicketTipo.Cerrado => (TipoNotificacion.Cerrado, $"Tu ticket #{ticket.Id} fue cerrado"),
            EstadoTicketTipo.Abierto when anterior is EstadoTicketTipo.Resuelto or EstadoTicketTipo.Cerrado =>
                (TipoNotificacion.Reabierto, $"Tu ticket #{ticket.Id} fue reabierto"),
            _ => (TipoNotificacion.CambioEstado, $"Tu ticket #{ticket.Id} cambió a {NombreEstado(nuevo)}")
        };

        return Una(ticket.ClienteId, ticket.Id, tipo, mensaje, clave);
    }

    public IReadOnlyList<NotificacionPendienteDto> PorComentario(Ticket ticket, Comentario comentario, UsuarioContextoDto autor)
    {
        if (comentario.EsNotaInterna)
            return Ninguna;

        var clave = $"comentario:{comentario.Id}";

        if (autor.Rol == RolTipo.Cliente)
        {
            return ticket.AgenteAsignadoId is int agenteId && agenteId != autor.UsuarioId
                ? Una(agenteId, ticket.Id, TipoNotificacion.RespuestaCliente, $"El cliente respondió en el ticket #{ticket.Id}", clave)
                : Ninguna;
        }

        if (ticket.ClienteId == autor.UsuarioId)
            return Ninguna;

        return comentario.SolicitaInformacion
            ? Una(ticket.ClienteId, ticket.Id, TipoNotificacion.SolicitudInformacion,
                $"Necesitamos información adicional sobre tu ticket #{ticket.Id}", clave)
            : Una(ticket.ClienteId, ticket.Id, TipoNotificacion.RespuestaEquipo,
                $"El equipo respondió a tu ticket #{ticket.Id}", clave);
    }

    private static IReadOnlyList<NotificacionPendienteDto> Una(int usuarioId, int ticketId, TipoNotificacion tipo, string mensaje, string clave) =>
        new[] { new NotificacionPendienteDto { UsuarioId = usuarioId, TicketId = ticketId, Tipo = tipo, Mensaje = mensaje, ClaveEvento = clave } };

    private static string NombreEstado(EstadoTicketTipo estado) => estado switch
    {
        EstadoTicketTipo.EnProgreso => "En progreso",
        _ => estado.ToString()
    };
}
