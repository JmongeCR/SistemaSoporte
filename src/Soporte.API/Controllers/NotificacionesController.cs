using Microsoft.AspNetCore.Mvc;
using Soporte.Abstracciones.DTOs;
using Soporte.Abstracciones.Interfaces.Flujo;
using Soporte.API.Servicios;

namespace Soporte.API.Controllers;

/// <summary>Notificaciones del usuario autenticado. No existe forma de consultar las de otro usuario.</summary>
[ApiController]
[Route("api/notificaciones")]
[Produces("application/json")]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
public class NotificacionesController : SoporteControllerBase
{
    private readonly INotificacionFlujo _notificacionFlujo;
    private readonly IUsuarioActualProveedor _usuarioActual;

    public NotificacionesController(INotificacionFlujo notificacionFlujo, IUsuarioActualProveedor usuarioActual)
    {
        _notificacionFlujo = notificacionFlujo;
        _usuarioActual = usuarioActual;
    }

    /// <summary>Notificaciones recientes, de la más nueva a la más vieja.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<NotificacionDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<NotificacionDto>>> ListarAsync([FromQuery] bool soloNoLeidas = false, [FromQuery] int cantidad = 20)
    {
        var usuario = _usuarioActual.Obtener();
        return usuario is null ? NoAutenticado() : Ok(await _notificacionFlujo.ListarAsync(usuario, soloNoLeidas, cantidad));
    }

    /// <summary>Cantidad de no leídas (para el contador de la campana).</summary>
    [HttpGet("resumen")]
    [ProducesResponseType(typeof(ResumenNotificacionesDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<ResumenNotificacionesDto>> ObtenerResumenAsync()
    {
        var usuario = _usuarioActual.Obtener();
        return usuario is null ? NoAutenticado() : Ok(await _notificacionFlujo.ObtenerResumenAsync(usuario));
    }

    /// <summary>Marca una notificación propia como leída. Una ajena responde 404.</summary>
    [HttpPost("{id:int}/leida")]
    [ProducesResponseType(typeof(NotificacionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<NotificacionDto>> MarcarLeidaAsync(int id)
    {
        var usuario = _usuarioActual.Obtener();
        if (usuario is null)
            return NoAutenticado();

        var resultado = await _notificacionFlujo.MarcarLeidaAsync(id, usuario);
        return resultado.Exito ? Ok(resultado.Datos) : RespuestaError(resultado);
    }

    /// <summary>Marca todas las notificaciones propias como leídas.</summary>
    [HttpPost("leidas")]
    [ProducesResponseType(typeof(ResumenNotificacionesDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<ResumenNotificacionesDto>> MarcarTodasLeidasAsync()
    {
        var usuario = _usuarioActual.Obtener();
        return usuario is null ? NoAutenticado() : Ok(await _notificacionFlujo.MarcarTodasLeidasAsync(usuario));
    }
}
