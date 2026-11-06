using Soporte.Abstracciones.DTOs;
using Soporte.Abstracciones.Interfaces.DA;
using Soporte.Abstracciones.Interfaces.Flujo;
using Soporte.Abstracciones.Modelos;

namespace Soporte.Flujo.Servicios;

public class PlantillaRespuestaFlujo : IPlantillaRespuestaFlujo
{
    private readonly IPlantillaRespuestaRepository _repo;

    public PlantillaRespuestaFlujo(IPlantillaRespuestaRepository repo) => _repo = repo;

    public Task<IReadOnlyList<PlantillaRespuestaDto>> ListarAsync() => _repo.ListarAsync();

    public async Task<ResultadoOperacion<PlantillaRespuestaDto>> CrearAsync(CrearPlantillaDto dto, int creadoPorId)
    {
        var entidad = new PlantillaRespuesta
        {
            Titulo      = dto.Titulo.Trim(),
            Texto       = dto.Texto.Trim(),
            CreadoPorId = creadoPorId,
            FechaCreacion = DateTime.UtcNow
        };
        var resultado = await _repo.CrearAsync(entidad);
        return ResultadoOperacion<PlantillaRespuestaDto>.Ok(resultado);
    }

    public async Task<ResultadoOperacion<PlantillaRespuestaDto>> ActualizarAsync(int id, CrearPlantillaDto dto)
    {
        var resultado = await _repo.ActualizarAsync(id, dto.Titulo.Trim(), dto.Texto.Trim());
        if (resultado is null)
            return ResultadoOperacion<PlantillaRespuestaDto>.NoEncontrado("Plantilla no encontrada.");
        return ResultadoOperacion<PlantillaRespuestaDto>.Ok(resultado);
    }

    public async Task<ResultadoOperacion> EliminarAsync(int id)
    {
        var ok = await _repo.EliminarAsync(id);
        return ok ? ResultadoOperacion.Ok() : ResultadoOperacion.NoEncontrado("Plantilla no encontrada.");
    }
}
