using Microsoft.AspNetCore.Mvc;
using Soporte.Abstracciones.DTOs;
using Soporte.Abstracciones.Interfaces.Flujo;
using Soporte.API.Servicios;

namespace Soporte.API.Controllers;

/// <summary>Archivos adjuntos de los tickets (HU-04 esc. 3, HU-11).</summary>
[ApiController]
[Produces("application/json")]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
public class AdjuntosController : SoporteControllerBase
{
    private readonly IAdjuntoFlujo _adjuntoFlujo;
    private readonly IUsuarioActualProveedor _usuarioActual;

    public AdjuntosController(IAdjuntoFlujo adjuntoFlujo, IUsuarioActualProveedor usuarioActual)
    {
        _adjuntoFlujo = adjuntoFlujo;
        _usuarioActual = usuarioActual;
    }

    /// <summary>Adjunta archivos a un ticket existente: hasta 5 PDF, PNG o JPG de máximo 10 MB.</summary>
    [HttpPost("api/tickets/{ticketId:int}/adjuntos")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(LimiteSubidaBytes)]
    [RequestFormLimits(MultipartBodyLengthLimit = LimiteSubidaBytes)]
    [ProducesResponseType(typeof(IReadOnlyList<AdjuntoDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IReadOnlyList<AdjuntoDto>>> AgregarAsync(int ticketId, [FromForm] List<IFormFile> archivos)
    {
        var usuario = _usuarioActual.Obtener();
        if (usuario is null)
            return NoAutenticado();

        var entrantes = ConvertirArchivos(archivos);
        try
        {
            var resultado = await _adjuntoFlujo.AgregarATicketAsync(ticketId, entrantes, usuario);
            return resultado.Exito ? StatusCode(StatusCodes.Status201Created, resultado.Datos) : RespuestaError(resultado);
        }
        finally
        {
            LiberarArchivos(entrantes);
        }
    }

    /// <summary>Descarga un adjunto si el usuario puede ver el ticket al que pertenece.</summary>
    [HttpGet("api/adjuntos/{id:int}")]
    [Produces("application/octet-stream", "application/json")]
    [ProducesResponseType(typeof(FileStreamResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> DescargarAsync(int id)
    {
        var usuario = _usuarioActual.Obtener();
        if (usuario is null)
            return NoAutenticado();

        var resultado = await _adjuntoFlujo.DescargarAsync(id, usuario);
        if (!resultado.Exito)
            return RespuestaError(resultado);

        var archivo = resultado.Datos!;
        return File(archivo.Contenido, archivo.TipoContenido, archivo.NombreOriginal);
    }
}
