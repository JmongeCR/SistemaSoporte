using Microsoft.AspNetCore.Mvc;
using Soporte.Abstracciones.DTOs;
using Soporte.Abstracciones.Interfaces.Flujo;
using Soporte.API.Servicios;

namespace Soporte.API.Controllers;

/// <summary>Perfil del usuario autenticado: actualizar nombre y contraseña.</summary>
[ApiController]
[Route("api/cuenta")]
[Produces("application/json")]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
public class CuentaController : SoporteControllerBase
{
    private readonly IUsuarioFlujo _usuarioFlujo;
    private readonly IUsuarioActualProveedor _usuarioActual;

    public CuentaController(IUsuarioFlujo usuarioFlujo, IUsuarioActualProveedor usuarioActual)
    {
        _usuarioFlujo = usuarioFlujo;
        _usuarioActual = usuarioActual;
    }

    /// <summary>Actualiza nombre y/o contraseña del usuario autenticado.</summary>
    [HttpPut("perfil")]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(UsuarioDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<UsuarioDto>> ActualizarPerfilAsync([FromBody] ActualizarPerfilDto dto)
    {
        var usuario = _usuarioActual.Obtener();
        if (usuario is null)
            return NoAutenticado();

        var resultado = await _usuarioFlujo.ActualizarPerfilAsync(usuario.UsuarioId, dto, usuario);
        return resultado.Exito ? Ok(resultado.Datos) : RespuestaError(resultado);
    }
}
