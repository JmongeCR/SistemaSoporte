using Soporte.Abstracciones.DTOs;
using Soporte.Abstracciones.Modelos;

namespace Soporte.Abstracciones.Interfaces.DA;

public interface INotificacionRepository
{
    /// <summary>Guarda las notificaciones omitiendo las que ya existen para el mismo destinatario y evento.</summary>
    /// <returns>Cantidad realmente guardada.</returns>
    Task<int> CrearAsync(IReadOnlyList<Notificacion> notificaciones);

    /// <summary>Siempre filtra por destinatario: nunca devuelve notificaciones de otro usuario.</summary>
    Task<IReadOnlyList<NotificacionDto>> ListarAsync(int usuarioId, bool soloNoLeidas, int cantidad);
    Task<int> ContarNoLeidasAsync(int usuarioId);

    /// <summary>Marca como leída solo si pertenece al usuario. Devuelve null si no existe o es ajena.</summary>
    Task<NotificacionDto?> MarcarLeidaAsync(int id, int usuarioId);
    Task<int> MarcarTodasLeidasAsync(int usuarioId);
}
