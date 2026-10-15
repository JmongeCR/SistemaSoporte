using Soporte.Abstracciones.DTOs;

namespace Soporte.Web.Servicios;

public interface ITicketsApiServicio
{
    /// <summary>GET /api/tickets con los filtros como query string (HU-05).</summary>
    Task<ResultadoOperacion<ResultadoPaginado<TicketResumenDto>>> ListarAsync(FiltroTicketsDto filtro);

    /// <summary>POST /api/tickets (multipart) con los adjuntos opcionales (HU-04).</summary>
    Task<ResultadoOperacion<TicketDetalleDto>> CrearAsync(CrearTicketDto ticket, IReadOnlyList<IFormFile> adjuntos);

    Task<ResultadoOperacion<TicketDetalleDto>> ObtenerAsync(int id);

    /// <summary>HU-07/HU-08: estado, prioridad, categoría, agente y mensaje público opcional.</summary>
    Task<ResultadoOperacion<TicketDetalleDto>> ActualizarAsync(int id, ActualizarTicketDto cambios);

    /// <summary>HU-06: el agente se asigna el ticket.</summary>
    Task<ResultadoOperacion> TomarAsync(int id);

    /// <summary>HU-09 a HU-11: respuesta, nota interna o solicitud de información, con adjuntos.</summary>
    Task<ResultadoOperacion<ComentarioDto>> ComentarAsync(int id, CrearComentarioDto comentario, IReadOnlyList<IFormFile> adjuntos);

    Task<ResultadoOperacion<ArchivoDescargaDto>> DescargarAdjuntoAsync(int adjuntoId);
}
