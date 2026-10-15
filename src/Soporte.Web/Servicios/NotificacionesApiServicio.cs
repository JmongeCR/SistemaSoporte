using Soporte.Abstracciones.DTOs;

namespace Soporte.Web.Servicios;

public class NotificacionesApiServicio : INotificacionesApiServicio
{
    private const string Ruta = "api/notificaciones";

    private readonly IApiSoporteCliente _api;

    public NotificacionesApiServicio(IApiSoporteCliente api)
    {
        _api = api;
    }

    public async Task<ResultadoOperacion<IReadOnlyList<NotificacionDto>>> ListarAsync(int cantidad, bool soloNoLeidas = false) =>
        await _api.GetAsync<IReadOnlyList<NotificacionDto>>($"{Ruta}?cantidad={cantidad}&soloNoLeidas={soloNoLeidas}");

    public async Task<ResultadoOperacion<ResumenNotificacionesDto>> ObtenerResumenAsync() =>
        await _api.GetAsync<ResumenNotificacionesDto>($"{Ruta}/resumen");

    public async Task<ResultadoOperacion<NotificacionDto>> MarcarLeidaAsync(int id) =>
        await _api.PostJsonAsync<NotificacionDto>($"{Ruta}/{id}/leida", new { });

    public async Task<ResultadoOperacion<ResumenNotificacionesDto>> MarcarTodasLeidasAsync() =>
        await _api.PostJsonAsync<ResumenNotificacionesDto>($"{Ruta}/leidas", new { });
}
