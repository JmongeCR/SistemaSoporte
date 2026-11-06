using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Soporte.Abstracciones.DTOs;
using Soporte.Abstracciones.Interfaces.Flujo;
using Soporte.API.Servicios;

namespace Soporte.API.Controllers;

[ApiController]
[Route("api/tickets/{ticketId:int}/csat")]
public class CsatController : SoporteControllerBase
{
    private readonly ICsatFlujo              _flujo;
    private readonly IUsuarioActualProveedor _usuarioActual;

    public CsatController(ICsatFlujo flujo, IUsuarioActualProveedor usuarioActual)
    {
        _flujo         = flujo;
        _usuarioActual = usuarioActual;
    }

    [HttpGet]
    [Authorize]
    public async Task<ActionResult<CalificacionCsatDto>> ObtenerAsync(int ticketId)
    {
        var csat = await _flujo.ObtenerPorTicketAsync(ticketId);
        return csat is null ? NotFound() : Ok(csat);
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<CalificacionCsatDto>> CalificarAsync(int ticketId, [FromBody] CrearCsatDto dto)
    {
        var usuario = _usuarioActual.Obtener();
        if (usuario is null) return NoAutenticado();

        var resultado = await _flujo.CalificarAsync(ticketId, dto, usuario);
        return resultado.Exito ? Ok(resultado.Datos) : RespuestaError(resultado);
    }
}
