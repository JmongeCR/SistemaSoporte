using Soporte.Abstracciones.DTOs;
using Soporte.Abstracciones.Modelos;

namespace Soporte.Abstracciones.Interfaces.DA;

public interface IBaseConocimientoRepository
{
    Task<IReadOnlyList<ArticuloKbResumenDto>> ListarAsync(int? categoriaId = null, bool incluyeNoBublicados = false);
    Task<ArticuloKbDto?> ObtenerPorSlugAsync(string slug, bool incluyeNoBublicados = false);
    Task<ArticuloKbDto?> ObtenerPorIdAsync(int id, bool incluyeNoBublicados = false);
    Task ActualizarPublicadoAsync(int id, bool publicado);
    Task<IReadOnlyList<ArticuloKbResumenDto>> ListarPorTicketAsync(int ticketId);
    Task VincularAsync(int ticketId, int articuloId, int usuarioId);
    Task DesvincularAsync(int ticketId, int articuloId);
    Task IncrementarVistasAsync(int id);
    Task<ArticuloKbDto> CrearAsync(ArticuloKb articulo);
    Task<bool> ExisteSlugAsync(string slug);
    Task<VotoKbResultadoDto> VotarAsync(int articuloId, int usuarioId, bool esUtil);
    Task<VotoKbResultadoDto> ObtenerVotosAsync(int articuloId, int? usuarioId);
}
