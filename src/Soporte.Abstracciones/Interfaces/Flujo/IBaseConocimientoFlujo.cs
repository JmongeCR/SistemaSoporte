using Soporte.Abstracciones.DTOs;

namespace Soporte.Abstracciones.Interfaces.Flujo;

public interface IBaseConocimientoFlujo
{
    Task<IReadOnlyList<ArticuloKbResumenDto>> ListarAsync(int? categoriaId = null, bool incluyeNoBublicados = false);
    Task<ResultadoOperacion<ArticuloKbDto>> ObtenerPorSlugAsync(string slug, bool incluyeNoBublicados = false);
    Task<ResultadoOperacion<ArticuloKbDto>> CambiarPublicadoAsync(int id, bool publicado, UsuarioContextoDto usuario);
    Task<IReadOnlyList<ArticuloKbResumenDto>> ListarPorTicketAsync(int ticketId);
    Task<ResultadoOperacion> VincularAsync(int ticketId, int articuloId, UsuarioContextoDto usuario);
    Task<ResultadoOperacion> DesvincularAsync(int ticketId, int articuloId, UsuarioContextoDto usuario);
    Task<ResultadoOperacion<ArticuloKbDto>> CrearAsync(CrearArticuloKbDto dto, UsuarioContextoDto usuario);
    Task<ResultadoOperacion<ArticuloKbDto>> ActualizarAsync(int id, ActualizarArticuloKbDto dto, UsuarioContextoDto usuario);
    Task<ResultadoOperacion> EliminarAsync(int id, UsuarioContextoDto usuario);
    Task<ResultadoOperacion<VotoKbResultadoDto>> VotarAsync(int articuloId, bool esUtil, UsuarioContextoDto usuario);
    Task<VotoKbResultadoDto> ObtenerVotosAsync(int articuloId, int? usuarioId);
}
