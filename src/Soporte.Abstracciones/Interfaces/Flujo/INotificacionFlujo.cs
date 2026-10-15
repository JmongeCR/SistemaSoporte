using Soporte.Abstracciones.DTOs;

namespace Soporte.Abstracciones.Interfaces.Flujo;

/// <summary>Notificaciones del usuario autenticado. Nunca accede a las de otro usuario.</summary>
public interface INotificacionFlujo
{
    Task<IReadOnlyList<NotificacionDto>> ListarAsync(UsuarioContextoDto usuario, bool soloNoLeidas, int cantidad);
    Task<ResumenNotificacionesDto> ObtenerResumenAsync(UsuarioContextoDto usuario);
    Task<ResultadoOperacion<NotificacionDto>> MarcarLeidaAsync(int id, UsuarioContextoDto usuario);
    Task<ResumenNotificacionesDto> MarcarTodasLeidasAsync(UsuarioContextoDto usuario);
}
