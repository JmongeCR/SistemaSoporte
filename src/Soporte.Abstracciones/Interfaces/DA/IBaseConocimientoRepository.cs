using Soporte.Abstracciones.DTOs;
using Soporte.Abstracciones.Modelos;

namespace Soporte.Abstracciones.Interfaces.DA;

public interface IBaseConocimientoRepository
{
    Task<IReadOnlyList<ArticuloKbResumenDto>> ListarAsync(int? categoriaId = null);
    Task<ArticuloKbDto?> ObtenerPorSlugAsync(string slug);
    Task<ArticuloKbDto?> ObtenerPorIdAsync(int id);
    Task<IReadOnlyList<ArticuloKbResumenDto>> ListarPorTicketAsync(int ticketId);
    Task VincularAsync(int ticketId, int articuloId, int usuarioId);
    Task DesvincularAsync(int ticketId, int articuloId);
    Task IncrementarVistasAsync(int id);
    Task<ArticuloKbDto> CrearAsync(ArticuloKb articulo);
    Task<bool> ExisteSlugAsync(string slug);
}
