using Soporte.Abstracciones.DTOs;

namespace Soporte.Abstracciones.Interfaces.Flujo;

public interface IPlantillaRespuestaFlujo
{
    Task<IReadOnlyList<PlantillaRespuestaDto>> ListarAsync(int usuarioId);
    Task<ResultadoOperacion<PlantillaRespuestaDto>> CrearAsync(CrearPlantillaDto dto, int creadoPorId);
    Task<ResultadoOperacion<PlantillaRespuestaDto>> ActualizarAsync(int id, CrearPlantillaDto dto, UsuarioContextoDto usuario);
    Task<ResultadoOperacion> EliminarAsync(int id, UsuarioContextoDto usuario);
}
