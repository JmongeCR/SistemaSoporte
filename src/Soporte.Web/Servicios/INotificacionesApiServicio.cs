using Soporte.Abstracciones.DTOs;

namespace Soporte.Web.Servicios;

public interface INotificacionesApiServicio
{
    Task<ResultadoOperacion<IReadOnlyList<NotificacionDto>>> ListarAsync(int cantidad, bool soloNoLeidas = false);
    Task<ResultadoOperacion<ResumenNotificacionesDto>> ObtenerResumenAsync();
    Task<ResultadoOperacion<NotificacionDto>> MarcarLeidaAsync(int id);
    Task<ResultadoOperacion<ResumenNotificacionesDto>> MarcarTodasLeidasAsync();
}
