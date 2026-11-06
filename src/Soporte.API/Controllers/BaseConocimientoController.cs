using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Soporte.Abstracciones.DTOs;
using Soporte.Abstracciones.Enumeraciones;
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
    public async Task<ActionResult<IReadOnlyList<ArticuloKbResumenDto>>> ListarAsync([FromQuery] int? categoriaId = null)
    {
        var usuario = _usuarioActual.Obtener();
        var esGestor = usuario is not null &&
            (usuario.Rol == RolTipo.Administrador || usuario.Rol == RolTipo.Agente);
        return Ok(await _flujo.ListarAsync(categoriaId, incluyeNoBublicados: esGestor));
    }

    [AllowAnonymous]
    [HttpGet("{slug}")]
    [ProducesResponseType(typeof(ArticuloKbDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ArticuloKbDto>> ObtenerAsync(string slug)
    {
        var usuario = _usuarioActual.Obtener();
        var esGestor = usuario is not null &&
            (usuario.Rol == RolTipo.Administrador || usuario.Rol == RolTipo.Agente);
        var resultado = await _flujo.ObtenerPorSlugAsync(slug, incluyeNoBublicados: esGestor);
        return resultado.Exito ? Ok(resultado.Datos) : NotFound();
    }

    [HttpPatch("{id:int}/publicado")]
    [ProducesResponseType(typeof(ArticuloKbDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ArticuloKbDto>> CambiarPublicadoAsync(int id, [FromBody] bool publicado)
    {
        var usuario = _usuarioActual.Obtener()!;
        var resultado = await _flujo.CambiarPublicadoAsync(id, publicado, usuario);
        return resultado switch
        {
            { Exito: true }                                          => Ok(resultado.Datos),
            { TipoError: TipoError.SinPermiso }                     => Forbid(),
            _                                                        => NotFound()
        };
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
