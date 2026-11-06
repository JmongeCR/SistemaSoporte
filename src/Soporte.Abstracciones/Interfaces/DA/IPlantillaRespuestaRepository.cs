using Soporte.Abstracciones.DTOs;
using Soporte.Abstracciones.Modelos;

namespace Soporte.Abstracciones.Interfaces.DA;

public interface IPlantillaRespuestaRepository
{
    Task<IReadOnlyList<PlantillaRespuestaDto>> ListarAsync(bool incluyeInactivas = false);
    Task<PlantillaRespuesta?> ObtenerAsync(int id);
    Task<PlantillaRespuestaDto> CrearAsync(PlantillaRespuesta plantilla);
    Task<PlantillaRespuestaDto?> ActualizarAsync(int id, string titulo, string texto);
    Task<bool> EliminarAsync(int id);
}
