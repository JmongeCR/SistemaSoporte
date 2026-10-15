using Soporte.Abstracciones.DTOs;
using Soporte.Abstracciones.Modelos;

namespace Soporte.Flujo.Servicios;

/// <summary>Convierte las notificaciones decididas por las reglas en entidades para guardar.</summary>
internal static class NotificacionesMapeo
{
    public static IReadOnlyList<Notificacion> AEntidades(IReadOnlyList<NotificacionPendienteDto> pendientes)
    {
        var ahora = DateTime.UtcNow;
        return pendientes.Select(p => new Notificacion
        {
            UsuarioId = p.UsuarioId,
            TicketId = p.TicketId,
            Tipo = p.Tipo,
            Mensaje = p.Mensaje,
            ClaveEvento = p.ClaveEvento,
            FechaCreacion = ahora
        }).ToList();
    }
}
