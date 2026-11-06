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

    public async Task<ResultadoOperacion<IReadOnlyList<PrioridadSlaDto>>> ListarPrioridadesConSlaAsync() =>
        await _api.GetAsync<IReadOnlyList<PrioridadSlaDto>>("api/catalogos/prioridades/sla");

    public async Task<ResultadoOperacion<PrioridadSlaDto>> ActualizarSlaAsync(int id, ActualizarSlaDto dto) =>
        await _api.PutJsonAsync<PrioridadSlaDto>($"api/catalogos/prioridades/{id}/sla", dto);

    public Task<ResultadoOperacion<CatalogoDto>> CrearCategoriaAsync(string nombre) =>
        _api.PostJsonAsync<CatalogoDto>("api/catalogos/categorias", nombre);

    public Task<ResultadoOperacion<CatalogoDto>> ActualizarCategoriaAsync(int id, string nombre) =>
        _api.PutJsonAsync<CatalogoDto>($"api/catalogos/categorias/{id}", nombre);

    public Task<ResultadoOperacion> EliminarCategoriaAsync(int id) =>
        _api.DeleteAsync($"api/catalogos/categorias/{id}");
}
