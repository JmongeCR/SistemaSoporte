using Microsoft.AspNetCore.Mvc;
using Soporte.Abstracciones.DTOs;
using Soporte.Abstracciones.Interfaces.Flujo;
using Soporte.API.Servicios;

namespace Soporte.API.Controllers;

/// <summary>Usuarios y roles (HU-14, HU-15) y búsqueda de clientes solicitantes.</summary>
[ApiController]
[Route("api/usuarios")]
[Produces("application/json")]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
public class UsuariosController : SoporteControllerBase
{
    private readonly IUsuarioFlujo _usuarioFlujo;
    private readonly IUsuarioActualProveedor _usuarioActual;

    public UsuariosController(IUsuarioFlujo usuarioFlujo, IUsuarioActualProveedor usuarioActual)
    {
        _usuarioFlujo = usuarioFlujo;
        _usuarioActual = usuarioActual;
    }

    /// <summary>Lista usuarios con su rol, con búsqueda por nombre/correo y filtro por rol (HU-14). Solo Administrador.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<UsuarioDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<UsuarioDto>>> ListarAsync([FromQuery] FiltroUsuariosDto filtro)
    {
        var usuario = _usuarioActual.Obtener();
        if (usuario is null)
            return NoAutenticado();

        var resultado = await _usuarioFlujo.ListarAsync(filtro, usuario);
        return resultado.Exito ? Ok(resultado.Datos) : RespuestaError(resultado);
    }

    /// <summary>Clientes activos para elegir el solicitante de un ticket. Administrador y Agente.</summary>
    [HttpGet("clientes")]
    [ProducesResponseType(typeof(IReadOnlyList<UsuarioDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<UsuarioDto>>> BuscarClientesAsync([FromQuery] string? texto)
    {
        var usuario = _usuarioActual.Obtener();
        if (usuario is null)
            return NoAutenticado();

        var resultado = await _usuarioFlujo.BuscarClientesAsync(texto, usuario);
        return resultado.Exito ? Ok(resultado.Datos) : RespuestaError(resultado);
    }

    /// <summary>Permisos efectivos del usuario (defaults del rol + overrides). Solo Administrador.</summary>
    [HttpGet("{id:int}/permisos")]
    [ProducesResponseType(typeof(MatrizPermisosDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MatrizPermisosDto>> ObtenerPermisosAsync(int id)
    {
        var usuario = _usuarioActual.Obtener();
        if (usuario is null)
            return NoAutenticado();

        var resultado = await _usuarioFlujo.ObtenerPermisosEfectivosAsync(id, usuario);
        return resultado.Exito ? Ok(resultado.Datos) : RespuestaError(resultado);
    }

    /// <summary>Guarda los permisos explícitos del usuario. Solo Administrador.</summary>
    [HttpPut("{id:int}/permisos")]
    [Consumes("application/json")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ActualizarPermisosAsync(int id, [FromBody] ActualizarPermisosUsuarioDto dto)
    {
        var usuario = _usuarioActual.Obtener();
        if (usuario is null)
            return NoAutenticado();

        var resultado = await _usuarioFlujo.ActualizarPermisosAsync(id, dto, usuario);
        return resultado.Exito ? NoContent() : RespuestaError(resultado);
    }

    /// <summary>Elimina los overrides y deja al usuario con los defaults del rol. Solo Administrador.</summary>
    [HttpDelete("{id:int}/permisos")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RestablecerPermisosAsync(int id)
    {
        var usuario = _usuarioActual.Obtener();
        if (usuario is null)
            return NoAutenticado();

        var resultado = await _usuarioFlujo.RestablecerPermisosAsync(id, usuario);
        return resultado.Exito ? NoContent() : RespuestaError(resultado);
    }

    /// <summary>Cambia el rol de un usuario (HU-15). Solo Administrador.</summary>
    /// <remarks>No permite quitar el rol Administrador al único administrador del sistema.</remarks>
    [HttpPut("{id:int}/rol")]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(UsuarioDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UsuarioDto>> CambiarRolAsync(int id, [FromBody] CambiarRolDto dto)
    {
        var usuario = _usuarioActual.Obtener();
        if (usuario is null)
            return NoAutenticado();

        var resultado = await _usuarioFlujo.CambiarRolAsync(id, dto, usuario);
        return resultado.Exito ? Ok(resultado.Datos) : RespuestaError(resultado);
    }
}
