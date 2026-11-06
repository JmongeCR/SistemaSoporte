using Soporte.Abstracciones.DTOs;
using Soporte.Abstracciones.Enumeraciones;
using Soporte.Abstracciones.Interfaces.DA;
using Soporte.Abstracciones.Interfaces.Flujo;
using Soporte.Abstracciones.Modelos;

namespace Soporte.Flujo.Servicios;

public class PlantillaRespuestaFlujo : IPlantillaRespuestaFlujo
{
    private readonly IPlantillaRespuestaRepository _repo;

    public PlantillaRespuestaFlujo(IPlantillaRespuestaRepository repo) => _repo = repo;

    public Task<IReadOnlyList<PlantillaRespuestaDto>> ListarAsync(int usuarioId) =>
        _repo.ListarAsync(usuarioId);

    public async Task<ResultadoOperacion<PlantillaRespuestaDto>> CrearAsync(CrearPlantillaDto dto, int creadoPorId)
    {
        var entidad = new PlantillaRespuesta
        {
            Titulo        = dto.Titulo.Trim(),
            Texto         = dto.Texto.Trim(),
            Compartida    = dto.Compartida,
            CreadoPorId   = creadoPorId,
            FechaCreacion = DateTime.UtcNow
        };
        var resultado = await _repo.CrearAsync(entidad);
        return ResultadoOperacion<PlantillaRespuestaDto>.Ok(resultado);
    }

    public async Task<ResultadoOperacion<PlantillaRespuestaDto>> ActualizarAsync(int id, CrearPlantillaDto dto, UsuarioContextoDto usuario)
    {
        var existente = await _repo.ObtenerAsync(id);
        if (existente is null)
            return ResultadoOperacion<PlantillaRespuestaDto>.NoEncontrado("Plantilla no encontrada.");

        if (usuario.Rol != RolTipo.Administrador && existente.CreadoPorId != usuario.UsuarioId)
            return ResultadoOperacion<PlantillaRespuestaDto>.SinPermiso("Solo podés editar tus propias plantillas.");

        var resultado = await _repo.ActualizarAsync(id, dto.Titulo.Trim(), dto.Texto.Trim(), dto.Compartida);
        if (resultado is null)
            return ResultadoOperacion<PlantillaRespuestaDto>.NoEncontrado("Plantilla no encontrada.");
        return ResultadoOperacion<PlantillaRespuestaDto>.Ok(resultado);
    }

    public async Task<ResultadoOperacion> EliminarAsync(int id, UsuarioContextoDto usuario)
    {
        var existente = await _repo.ObtenerAsync(id);
        if (existente is null)
            return ResultadoOperacion.NoEncontrado("Plantilla no encontrada.");

        if (usuario.Rol != RolTipo.Administrador && existente.CreadoPorId != usuario.UsuarioId)
            return ResultadoOperacion.SinPermiso("Solo podés eliminar tus propias plantillas.");

        var ok = await _repo.EliminarAsync(id);
        return ok ? ResultadoOperacion.Ok() : ResultadoOperacion.NoEncontrado("Plantilla no encontrada.");
    }
}
