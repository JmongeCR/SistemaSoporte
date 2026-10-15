using Soporte.Abstracciones.DTOs;

namespace Soporte.Web.Servicios;

public class CatalogosApiServicio : ICatalogosApiServicio
{
    private readonly IApiSoporteCliente _api;

    public CatalogosApiServicio(IApiSoporteCliente api)
    {
        _api = api;
    }

    public async Task<ResultadoOperacion<IReadOnlyList<CatalogoDto>>> ListarEstadosAsync() =>
        await _api.GetAsync<IReadOnlyList<CatalogoDto>>("api/catalogos/estados");

    public async Task<ResultadoOperacion<IReadOnlyList<CatalogoDto>>> ListarPrioridadesAsync() =>
        await _api.GetAsync<IReadOnlyList<CatalogoDto>>("api/catalogos/prioridades");

    public async Task<ResultadoOperacion<IReadOnlyList<CatalogoDto>>> ListarCategoriasAsync() =>
        await _api.GetAsync<IReadOnlyList<CatalogoDto>>("api/catalogos/categorias");

    public async Task<ResultadoOperacion<IReadOnlyList<CatalogoDto>>> ListarRolesAsync() =>
        await _api.GetAsync<IReadOnlyList<CatalogoDto>>("api/catalogos/roles");

    public async Task<ResultadoOperacion<MatrizPermisosDto>> ObtenerMatrizPermisosAsync(int rolId) =>
        await _api.GetAsync<MatrizPermisosDto>($"api/catalogos/roles/{rolId}/permisos");
}
