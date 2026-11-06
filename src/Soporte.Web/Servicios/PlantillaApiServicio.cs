using Soporte.Abstracciones.DTOs;

namespace Soporte.Web.Servicios;

public class PlantillaApiServicio : IPlantillaApiServicio
{
    private const string Ruta = "api/plantillas";
    private readonly IApiSoporteCliente _api;

    public PlantillaApiServicio(IApiSoporteCliente api) => _api = api;

    public Task<ResultadoOperacion<IReadOnlyList<PlantillaRespuestaDto>>> ListarAsync() =>
        _api.GetAsync<IReadOnlyList<PlantillaRespuestaDto>>(Ruta);

    public Task<ResultadoOperacion<PlantillaRespuestaDto>> CrearAsync(CrearPlantillaDto dto) =>
        _api.PostJsonAsync<PlantillaRespuestaDto>(Ruta, dto);

    public Task<ResultadoOperacion<PlantillaRespuestaDto>> ActualizarAsync(int id, CrearPlantillaDto dto) =>
        _api.PutJsonAsync<PlantillaRespuestaDto>($"{Ruta}/{id}", dto);

    public Task<ResultadoOperacion> EliminarAsync(int id) =>
        _api.DeleteAsync($"{Ruta}/{id}");
}
