using Soporte.Abstracciones.DTOs;
using Soporte.Abstracciones.Interfaces.DA;
using Soporte.Abstracciones.Interfaces.Flujo;

namespace Soporte.Flujo.Servicios;

public class NotificacionFlujo : INotificacionFlujo
{
    private const int MaximoPorConsulta = 50;

    private readonly INotificacionRepository _notificaciones;

    public NotificacionFlujo(INotificacionRepository notificaciones)
    {
        _notificaciones = notificaciones;
    }

    public async Task<IReadOnlyList<NotificacionDto>> ListarAsync(UsuarioContextoDto usuario, bool soloNoLeidas, int cantidad)
    {
        return await _notificaciones.ListarAsync(usuario.UsuarioId, soloNoLeidas, Math.Clamp(cantidad, 1, MaximoPorConsulta));
    }

    public async Task<ResumenNotificacionesDto> ObtenerResumenAsync(UsuarioContextoDto usuario)
    {
        return new ResumenNotificacionesDto { NoLeidas = await _notificaciones.ContarNoLeidasAsync(usuario.UsuarioId) };
    }

    public async Task<ResultadoOperacion<NotificacionDto>> MarcarLeidaAsync(int id, UsuarioContextoDto usuario)
    {
        // Una notificación ajena se reporta como inexistente, para no revelar que existe.
        var notificacion = await _notificaciones.MarcarLeidaAsync(id, usuario.UsuarioId);
        return notificacion is null
            ? ResultadoOperacion<NotificacionDto>.NoEncontrado($"No existe la notificación #{id}.")
            : ResultadoOperacion<NotificacionDto>.Ok(notificacion);
    }

    public async Task<ResumenNotificacionesDto> MarcarTodasLeidasAsync(UsuarioContextoDto usuario)
    {
        await _notificaciones.MarcarTodasLeidasAsync(usuario.UsuarioId);
        return await ObtenerResumenAsync(usuario);
    }
}
