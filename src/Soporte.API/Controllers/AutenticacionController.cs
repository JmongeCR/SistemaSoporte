using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Soporte.Abstracciones.DTOs;
using Soporte.Abstracciones.Interfaces.Flujo;

namespace Soporte.API.Controllers;

/// <summary>Inicio de sesión y recuperación de contraseña (HU-01, HU-02). No requiere token.</summary>
[ApiController]
[Route("api/autenticacion")]
[AllowAnonymous]
[EnableRateLimiting(Program.NombrePoliticaAuth)]
[Produces("application/json")]
[Consumes("application/json")]
public class AutenticacionController : SoporteControllerBase
{
    private readonly IAutenticacionFlujo _autenticacionFlujo;

    public AutenticacionController(IAutenticacionFlujo autenticacionFlujo)
    {
        _autenticacionFlujo = autenticacionFlujo;
    }

    /// <summary>Valida correo y contraseña contra la base de datos y devuelve el token de acceso (HU-01).</summary>
    /// <remarks>Usar el token en Swagger con el botón "Authorize".</remarks>
    [HttpPost("login")]
    [ProducesResponseType(typeof(SesionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<SesionDto>> IniciarSesionAsync([FromBody] LoginSolicitudDto dto)
    {
        var resultado = await _autenticacionFlujo.IniciarSesionAsync(dto);
        return resultado.Exito ? Ok(resultado.Datos) : RespuestaError(resultado);
    }

    /// <summary>Envía un enlace de restablecimiento válido por 24 horas (HU-02).</summary>
    /// <remarks>Responde siempre el mismo mensaje, exista o no el correo.</remarks>
    [HttpPost("recuperacion")]
    [ProducesResponseType(typeof(MensajeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<MensajeDto>> SolicitarRecuperacionAsync([FromBody] SolicitudRecuperacionDto dto)
    {
        var resultado = await _autenticacionFlujo.SolicitarRecuperacionAsync(dto);
        return resultado.Exito ? Ok(new MensajeDto { Mensaje = resultado.Mensaje! }) : RespuestaError(resultado);
    }

    /// <summary>Define una nueva contraseña con el token recibido por correo (HU-02).</summary>
    [HttpPost("restablecimiento")]
    [ProducesResponseType(typeof(MensajeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<MensajeDto>> RestablecerContrasenaAsync([FromBody] RestablecerContrasenaDto dto)
    {
        var resultado = await _autenticacionFlujo.RestablecerContrasenaAsync(dto);
        return resultado.Exito ? Ok(new MensajeDto { Mensaje = resultado.Mensaje! }) : RespuestaError(resultado);
    }
}
