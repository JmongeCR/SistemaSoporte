using Microsoft.AspNetCore.Mvc;
using Soporte.Abstracciones.DTOs;
using Soporte.Abstracciones.Interfaces.Flujo;
using Soporte.API.Servicios;

namespace Soporte.API.Controllers;

/// <summary>Conversación del ticket: respuestas públicas, notas internas y solicitudes de información (HU-09 a HU-11).</summary>
[ApiController]
[Route("api/tickets/{ticketId:int}/comentarios")]
[Produces("application/json")]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
public class ComentariosController : SoporteControllerBase
{
    private readonly IComentarioFlujo _comentarioFlujo;
    private readonly IUsuarioActualProveedor _usuarioActual;

    public ComentariosController(IComentarioFlujo comentarioFlujo, IUsuarioActualProveedor usuarioActual)
    {
        _comentarioFlujo = comentarioFlujo;
        _usuarioActual = usuarioActual;
    }

    /// <summary>Comentarios del ticket. Las notas internas solo se incluyen para Agentes y Administradores.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<ComentarioDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ComentarioDto>>> ListarAsync(int ticketId)
    {
        var usuario = _usuarioActual.Obtener();
        if (usuario is null)
            return NoAutenticado();

        var resultado = await _comentarioFlujo.ListarAsync(ticketId, usuario);
        return resultado.Exito ? Ok(resultado.Datos) : RespuestaError(resultado);
    }

    /// <summary>Agrega una respuesta, nota interna o solicitud de información, con adjuntos opcionales.</summary>
    /// <remarks>
    /// El autor es siempre el usuario del token. EsNotaInterna y SolicitaInformacion solo para el equipo.
    /// Un ticket Cerrado no admite respuestas públicas (HU-08 esc. 3).
    /// </remarks>
    [HttpPost]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(LimiteSubidaBytes)]
    [RequestFormLimits(MultipartBodyLengthLimit = LimiteSubidaBytes)]
    [ProducesResponseType(typeof(ComentarioDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ComentarioDto>> CrearAsync(int ticketId, [FromForm] CrearComentarioDto dto, [FromForm] List<IFormFile>? adjuntos)
    {
        var usuario = _usuarioActual.Obtener();
        if (usuario is null)
            return NoAutenticado();

        var archivos = ConvertirArchivos(adjuntos);
        try
        {
            var resultado = await _comentarioFlujo.CrearAsync(ticketId, dto, archivos, usuario);
            return resultado.Exito ? StatusCode(StatusCodes.Status201Created, resultado.Datos) : RespuestaError(resultado);
        }
        finally
        {
            LiberarArchivos(archivos);
        }
    }
}
