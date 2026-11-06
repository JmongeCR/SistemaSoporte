using Soporte.Abstracciones.DTOs;

namespace Soporte.Web.Servicios;

public interface IBaseConocimientoApiServicio
{
    Task<ResultadoOperacion<IReadOnlyList<ArticuloKbResumenDto>>> ListarAsync(int? categoriaId = null);
    Task<ResultadoOperacion<ArticuloKbDto>> ObtenerPorSlugAsync(string slug);
    Task<ResultadoOperacion<IReadOnlyList<ArticuloKbResumenDto>>> ListarPorTicketAsync(int ticketId);
    Task<ResultadoOperacion> VincularAsync(int ticketId, int articuloId);
    Task<ResultadoOperacion> DesvincularAsync(int ticketId, int articuloId);
    Task<ResultadoOperacion<ArticuloKbDto>> CrearAsync(CrearArticuloKbDto dto);
    Task<ResultadoOperacion<ArticuloKbDto>> CambiarPublicadoAsync(int id, bool publicado);
    Task<ResultadoOperacion<VotoKbResultadoDto>> VotarAsync(int articuloId, bool esUtil);
    Task<ResultadoOperacion<VotoKbResultadoDto>> ObtenerVotosAsync(int articuloId);
}
