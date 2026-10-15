using Microsoft.AspNetCore.Mvc;
using Soporte.Abstracciones.DTOs;
using Soporte.Abstracciones.Interfaces.Flujo;
using Soporte.API.Servicios;

namespace Soporte.API.Controllers;

/// <summary>Gestión de tickets de soporte (HU-04 a HU-08).</summary>
[ApiController]
[Route("api/tickets")]
[Produces("application/json")]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
public class TicketsController : SoporteControllerBase
{
    private const string RutaObtener = "ObtenerTicket";

    private readonly ITicketFlujo _ticketFlujo;
    private readonly IUsuarioActualProveedor _usuarioActual;

    public TicketsController(ITicketFlujo ticketFlujo, IUsuarioActualProveedor usuarioActual)
    {
        _ticketFlujo = ticketFlujo;
        _usuarioActual = usuarioActual;
    }

    /// <summary>Lista los tickets visibles para el usuario, con filtros y paginación (HU-05).</summary>
    /// <remarks>Cliente: sus tickets. Agente: los asignados a él y los sin asignar. Administrador: todos.</remarks>
    [HttpGet]
    [ProducesResponseType(typeof(ResultadoPaginado<TicketResumenDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ResultadoPaginado<TicketResumenDto>>> ListarAsync([FromQuery] FiltroTicketsDto filtro)
    {
        var usuario = _usuarioActual.Obtener();
        if (usuario is null)
            return NoAutenticado();

        return Ok(await _ticketFlujo.ListarAsync(filtro, usuario));
    }

    /// <summary>Obtiene el detalle de un ticket con comentarios, adjuntos e historial.</summary>
    /// <remarks>Las notas internas solo se incluyen para Agentes y Administradores (HU-10).</remarks>
    [HttpGet("{id:int}", Name = RutaObtener)]
    [ProducesResponseType(typeof(TicketDetalleDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TicketDetalleDto>> ObtenerAsync(int id)
    {
        var usuario = _usuarioActual.Obtener();
        if (usuario is null)
            return NoAutenticado();

        var resultado = await _ticketFlujo.ObtenerAsync(id, usuario);
        return resultado.Exito ? Ok(resultado.Datos) : RespuestaError(resultado);
    }

    /// <summary>Crea un ticket en estado Abierto y sin agente asignado (HU-04).</summary>
    /// <remarks>
    /// El Cliente lo crea a su nombre; Administrador y Agente indican el ClienteId solicitante.
    /// Adjuntos opcionales (HU-04 esc. 3): hasta 5 archivos PDF, PNG o JPG de máximo 10 MB.
    /// </remarks>
    [HttpPost]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(LimiteSubidaBytes)]
    [RequestFormLimits(MultipartBodyLengthLimit = LimiteSubidaBytes)]
    [ProducesResponseType(typeof(TicketDetalleDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<TicketDetalleDto>> CrearAsync([FromForm] CrearTicketDto dto, [FromForm] List<IFormFile>? adjuntos)
    {
        var usuario = _usuarioActual.Obtener();
        if (usuario is null)
            return NoAutenticado();

        var archivos = ConvertirArchivos(adjuntos);
        try
        {
            var resultado = await _ticketFlujo.CrearAsync(dto, archivos, usuario);
            if (!resultado.Exito)
                return RespuestaError(resultado);

            return CreatedAtRoute(RutaObtener, new { id = resultado.Datos!.Id }, resultado.Datos);
        }
        finally
        {
            LiberarArchivos(archivos);
        }
    }

    /// <summary>Cambia estado, prioridad, categoría o agente asignado (HU-07, HU-08).</summary>
    /// <remarks>
    /// Enviar el RowVersion recibido al consultar el ticket; si otro usuario lo modificó
    /// mientras tanto, se responde 409. Solo el Administrador reasigna a otro agente.
    /// </remarks>
    [HttpPut("{id:int}")]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(TicketDetalleDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<TicketDetalleDto>> ActualizarAsync(int id, [FromBody] ActualizarTicketDto dto)
    {
        var usuario = _usuarioActual.Obtener();
        if (usuario is null)
            return NoAutenticado();

        var resultado = await _ticketFlujo.ActualizarAsync(id, dto, usuario);
        return resultado.Exito ? Ok(resultado.Datos) : RespuestaError(resultado);
    }

    /// <summary>El agente se asigna un ticket sin asignar (HU-06). Solo Agentes.</summary>
    [HttpPost("{id:int}/tomar")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> TomarAsync(int id)
    {
        var usuario = _usuarioActual.Obtener();
        if (usuario is null)
            return NoAutenticado();

        var resultado = await _ticketFlujo.TomarAsync(id, usuario);
        return resultado.Exito ? NoContent() : RespuestaError(resultado);
    }
}
