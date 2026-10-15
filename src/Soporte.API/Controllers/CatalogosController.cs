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
