using Microsoft.AspNetCore.Mvc;
using Soporte.Abstracciones.DTOs;
using Soporte.Abstracciones.Interfaces.Flujo;
using Soporte.API.Servicios;

namespace Soporte.API.Controllers;

/// <summary>Catálogos para listas desplegables y matriz de permisos (HU-16).</summary>
[ApiController]
[Route("api/catalogos")]
[Produces("application/json")]
public class CatalogosController : SoporteControllerBase
{
    private readonly ICatalogoFlujo _catalogoFlujo;
    private readonly IUsuarioActualProveedor _usuarioActual;

    public CatalogosController(ICatalogoFlujo catalogoFlujo, IUsuarioActualProveedor usuarioActual)
    {
        _catalogoFlujo = catalogoFlujo;
        _usuarioActual = usuarioActual;
    }

    /// <summary>Categorías activas.</summary>
    [HttpGet("categorias")]
    [ProducesResponseType(typeof(IReadOnlyList<CatalogoDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<CatalogoDto>>> ListarCategoriasAsync() =>
        Ok(await _catalogoFlujo.ListarCategoriasAsync());

    /// <summary>Prioridades (Baja, Media, Alta, Urgente).</summary>
    [HttpGet("prioridades")]
    [ProducesResponseType(typeof(IReadOnlyList<CatalogoDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<CatalogoDto>>> ListarPrioridadesAsync() =>
        Ok(await _catalogoFlujo.ListarPrioridadesAsync());

    /// <summary>Estados del ticket (Abierto, En Progreso, Resuelto, Cerrado).</summary>
    [HttpGet("estados")]
    [ProducesResponseType(typeof(IReadOnlyList<CatalogoDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<CatalogoDto>>> ListarEstadosAsync() =>
        Ok(await _catalogoFlujo.ListarEstadosAsync());

    /// <summary>Roles del sistema.</summary>
    [HttpGet("roles")]
    [ProducesResponseType(typeof(IReadOnlyList<CatalogoDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<CatalogoDto>>> ListarRolesAsync() =>
        Ok(await _catalogoFlujo.ListarRolesAsync());

    /// <summary>Prioridades con sus horas de SLA configuradas. Solo Administrador.</summary>
    [HttpGet("prioridades/sla")]
    [ProducesResponseType(typeof(IReadOnlyList<PrioridadSlaDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IReadOnlyList<PrioridadSlaDto>>> ListarPrioridadesConSlaAsync()
    {
        var usuario = _usuarioActual.Obtener();
        if (usuario is null) return NoAutenticado();
        if (usuario.Rol != Soporte.Abstracciones.Enumeraciones.RolTipo.Administrador)
            return Forbid();
        return Ok(await _catalogoFlujo.ListarPrioridadesConSlaAsync());
    }

    /// <summary>Actualiza las horas de SLA de una prioridad. Solo Administrador.</summary>
    [HttpPut("prioridades/{id:int}/sla")]
    [ProducesResponseType(typeof(PrioridadSlaDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PrioridadSlaDto>> ActualizarSlaAsync(int id, [FromBody] ActualizarSlaDto dto)
    {
        var usuario = _usuarioActual.Obtener();
        if (usuario is null) return NoAutenticado();
        var resultado = await _catalogoFlujo.ActualizarSlaAsync(id, dto, usuario);
        return resultado.Exito ? Ok(resultado.Datos) : RespuestaError(resultado);
    }

    /// <summary>Crea una categoría. Solo Administrador.</summary>
    [HttpPost("categorias")]
    [ProducesResponseType(typeof(CatalogoDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<CatalogoDto>> CrearCategoriaAsync([FromBody] string nombre)
    {
        var usuario = _usuarioActual.Obtener();
        if (usuario is null) return NoAutenticado();
        var resultado = await _catalogoFlujo.CrearCategoriaAsync(nombre, usuario);
        if (!resultado.Exito) return RespuestaError(resultado);
        return CreatedAtAction(nameof(CrearCategoriaAsync), resultado.Datos);
    }

    /// <summary>Renombra una categoría. Solo Administrador.</summary>
    [HttpPut("categorias/{id:int}")]
    [ProducesResponseType(typeof(CatalogoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CatalogoDto>> ActualizarCategoriaAsync(int id, [FromBody] string nombre)
    {
        var usuario = _usuarioActual.Obtener();
        if (usuario is null) return NoAutenticado();
        var resultado = await _catalogoFlujo.ActualizarCategoriaAsync(id, nombre, usuario);
        return resultado.Exito ? Ok(resultado.Datos) : RespuestaError(resultado);
    }

    /// <summary>Elimina una categoría sin uso. Solo Administrador.</summary>
    [HttpDelete("categorias/{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> EliminarCategoriaAsync(int id)
    {
        var usuario = _usuarioActual.Obtener();
        if (usuario is null) return NoAutenticado();
        var resultado = await _catalogoFlujo.EliminarCategoriaAsync(id, usuario);
        return resultado.Exito ? NoContent() : RespuestaError(resultado);
    }

    /// <summary>Permisos del rol agrupados, de solo lectura (HU-16). Solo Administrador.</summary>
    [HttpGet("roles/{rolId:int}/permisos")]
    [ProducesResponseType(typeof(MatrizPermisosDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MatrizPermisosDto>> ObtenerMatrizPermisosAsync(int rolId)
    {
        var usuario = _usuarioActual.Obtener();
        if (usuario is null)
            return NoAutenticado();

        var resultado = await _catalogoFlujo.ObtenerMatrizPermisosAsync(rolId, usuario);
        return resultado.Exito ? Ok(resultado.Datos) : RespuestaError(resultado);
    }
}
