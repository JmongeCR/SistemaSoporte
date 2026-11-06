using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Soporte.Abstracciones.DTOs;
using Soporte.Abstracciones.Enumeraciones;
using Soporte.Abstracciones.Interfaces.Flujo;
using Soporte.API.Servicios;

namespace Soporte.API.Controllers;

[ApiController]
[Route("api/plantillas")]
[Authorize]
public class PlantillasController : SoporteControllerBase
{
    private readonly IPlantillaRespuestaFlujo _flujo;
    private readonly IUsuarioActualProveedor  _usuarioActual;

    public PlantillasController(IPlantillaRespuestaFlujo flujo, IUsuarioActualProveedor usuarioActual)
    {
        _flujo         = flujo;
        _usuarioActual = usuarioActual;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<PlantillaRespuestaDto>>> ListarAsync()
    {
        var plantillas = await _flujo.ListarAsync();
        return Ok(plantillas);
    }

    [HttpPost]
    public async Task<ActionResult<PlantillaRespuestaDto>> CrearAsync([FromBody] CrearPlantillaDto dto)
    {
        var usuario = _usuarioActual.Obtener();
        if (usuario is null) return NoAutenticado();
        if (usuario.Rol != RolTipo.Administrador) return Forbid();

        var resultado = await _flujo.CrearAsync(dto, usuario.UsuarioId);
        return resultado.Exito ? Ok(resultado.Datos) : RespuestaError(resultado);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<PlantillaRespuestaDto>> ActualizarAsync(int id, [FromBody] CrearPlantillaDto dto)
    {
        var usuario = _usuarioActual.Obtener();
        if (usuario is null) return NoAutenticado();
        if (usuario.Rol != RolTipo.Administrador) return Forbid();

        var resultado = await _flujo.ActualizarAsync(id, dto);
        return resultado.Exito ? Ok(resultado.Datos) : RespuestaError(resultado);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> EliminarAsync(int id)
    {
        var usuario = _usuarioActual.Obtener();
        if (usuario is null) return NoAutenticado();
        if (usuario.Rol != RolTipo.Administrador) return Forbid();

        var resultado = await _flujo.EliminarAsync(id);
        return resultado.Exito ? NoContent() : RespuestaError(resultado);
    }
}
