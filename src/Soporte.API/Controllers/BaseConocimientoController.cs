using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Soporte.Abstracciones.DTOs;
using Soporte.Abstracciones.Interfaces.Flujo;
using Soporte.API.Servicios;

namespace Soporte.API.Controllers;

[ApiController]
[Route("api/kb")]
[Produces("application/json")]
public class BaseConocimientoController : SoporteControllerBase
{
    private readonly IBaseConocimientoFlujo _flujo;
    private readonly IUsuarioActualProveedor _usuarioActual;

    public BaseConocimientoController(IBaseConocimientoFlujo flujo, IUsuarioActualProveedor usuarioActual)
    {
        _flujo = flujo;
        _usuarioActual = usuarioActual;
    }

    [AllowAnonymous]
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<ArticuloKbResumenDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ArticuloKbResumenDto>>> ListarAsync([FromQuery] int? categoriaId = null) =>
        Ok(await _flujo.ListarAsync(categoriaId));

    [AllowAnonymous]
    [HttpGet("{slug}")]
    [ProducesResponseType(typeof(ArticuloKbDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ArticuloKbDto>> ObtenerAsync(string slug)
    {
        var resultado = await _flujo.ObtenerPorSlugAsync(slug);
        return resultado.Exito ? Ok(resultado.Datos) : NotFound();
    }

    [AllowAnonymous]
    [HttpGet("ticket/{ticketId:int}")]
    [ProducesResponseType(typeof(IReadOnlyList<ArticuloKbResumenDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ArticuloKbResumenDto>>> ListarPorTicketAsync(int ticketId) =>
        Ok(await _flujo.ListarPorTicketAsync(ticketId));

    [HttpPost]
    [ProducesResponseType(typeof(ArticuloKbDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<ArticuloKbDto>> CrearAsync([FromBody] CrearArticuloKbDto dto)
    {
        var usuario = _usuarioActual.Obtener();
        var resultado = await _flujo.CrearAsync(dto, usuario);
        return CreatedAtAction(nameof(ObtenerAsync), new { slug = resultado.Datos!.Slug }, resultado.Datos);
    }

    [HttpPost("ticket/{ticketId:int}/{articuloId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> VincularAsync(int ticketId, int articuloId)
    {
        var usuario = _usuarioActual.Obtener();
        await _flujo.VincularAsync(ticketId, articuloId, usuario);
        return NoContent();
    }

    [HttpDelete("ticket/{ticketId:int}/{articuloId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DesvincularAsync(int ticketId, int articuloId)
    {
        var usuario = _usuarioActual.Obtener();
        await _flujo.DesvincularAsync(ticketId, articuloId, usuario);
        return NoContent();
    }
}
