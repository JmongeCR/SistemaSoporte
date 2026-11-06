using Soporte.Abstracciones.DTOs;

namespace Soporte.Abstracciones.Interfaces.Flujo;

public interface IBaseConocimientoFlujo
{
    Task<IReadOnlyList<ArticuloKbResumenDto>> ListarAsync(int? categoriaId = null);
    Task<ResultadoOperacion<ArticuloKbDto>> ObtenerPorSlugAsync(string slug);
    Task<IReadOnlyList<ArticuloKbResumenDto>> ListarPorTicketAsync(int ticketId);
    Task<ResultadoOperacion> VincularAsync(int ticketId, int articuloId, UsuarioContextoDto usuario);
    Task<ResultadoOperacion> DesvincularAsync(int ticketId, int articuloId, UsuarioContextoDto usuario);
    Task<ResultadoOperacion<ArticuloKbDto>> CrearAsync(CrearArticuloKbDto dto, UsuarioContextoDto usuario);
}
