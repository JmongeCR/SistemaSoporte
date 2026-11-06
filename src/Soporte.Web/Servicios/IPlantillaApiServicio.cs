using Soporte.Abstracciones.DTOs;

namespace Soporte.Web.Servicios;

public interface IPlantillaApiServicio
{
    Task<ResultadoOperacion<IReadOnlyList<PlantillaRespuestaDto>>> ListarAsync();
    Task<ResultadoOperacion<PlantillaRespuestaDto>> CrearAsync(CrearPlantillaDto dto);
    Task<ResultadoOperacion<PlantillaRespuestaDto>> ActualizarAsync(int id, CrearPlantillaDto dto);
    Task<ResultadoOperacion> EliminarAsync(int id);
}
