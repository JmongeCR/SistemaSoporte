using Soporte.Abstracciones.DTOs;

namespace Soporte.Web.Servicios;

public class BaseConocimientoApiServicio : IBaseConocimientoApiServicio
{
    private readonly IApiSoporteCliente _api;

    public BaseConocimientoApiServicio(IApiSoporteCliente api)
    {
        _api = api;
    }

    public Task<ResultadoOperacion<IReadOnlyList<ArticuloKbResumenDto>>> ListarAsync(int? categoriaId = null)
    {
        var url = categoriaId.HasValue
            ? $"api/kb?categoriaId={categoriaId.Value}"
            : "api/kb";
        return _api.GetAsync<IReadOnlyList<ArticuloKbResumenDto>>(url);
    }

    public Task<ResultadoOperacion<ArticuloKbDto>> ObtenerPorSlugAsync(string slug) =>
        _api.GetAsync<ArticuloKbDto>($"api/kb/{slug}");

    public Task<ResultadoOperacion<IReadOnlyList<ArticuloKbResumenDto>>> ListarPorTicketAsync(int ticketId) =>
        _api.GetAsync<IReadOnlyList<ArticuloKbResumenDto>>($"api/kb/ticket/{ticketId}");

    public Task<ResultadoOperacion> VincularAsync(int ticketId, int articuloId) =>
        _api.PostSinCuerpoAsync($"api/kb/ticket/{ticketId}/{articuloId}");

    public Task<ResultadoOperacion> DesvincularAsync(int ticketId, int articuloId) =>
        _api.DeleteAsync($"api/kb/ticket/{ticketId}/{articuloId}");
}
