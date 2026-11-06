using Soporte.Abstracciones.DTOs;
using Soporte.Abstracciones.Interfaces.DA;
using Soporte.Abstracciones.Interfaces.Flujo;

namespace Soporte.Flujo.Servicios;

public class BaseConocimientoFlujo : IBaseConocimientoFlujo
{
    private readonly IBaseConocimientoRepository _repo;

    public BaseConocimientoFlujo(IBaseConocimientoRepository repo)
    {
        _repo = repo;
    }

    public Task<IReadOnlyList<ArticuloKbResumenDto>> ListarAsync(int? categoriaId = null) =>
        _repo.ListarAsync(categoriaId);

    public async Task<ResultadoOperacion<ArticuloKbDto>> ObtenerPorSlugAsync(string slug)
    {
        var articulo = await _repo.ObtenerPorSlugAsync(slug);
        if (articulo is null)
            return ResultadoOperacion<ArticuloKbDto>.NoEncontrado("Artículo no encontrado.");

        await _repo.IncrementarVistasAsync(articulo.Id);
        return ResultadoOperacion<ArticuloKbDto>.Ok(articulo);
    }

    public Task<IReadOnlyList<ArticuloKbResumenDto>> ListarPorTicketAsync(int ticketId) =>
        _repo.ListarPorTicketAsync(ticketId);

    public async Task<ResultadoOperacion> VincularAsync(int ticketId, int articuloId, UsuarioContextoDto usuario)
    {
        await _repo.VincularAsync(ticketId, articuloId, usuario.UsuarioId);
        return ResultadoOperacion.Ok();
    }

    public async Task<ResultadoOperacion> DesvincularAsync(int ticketId, int articuloId, UsuarioContextoDto usuario)
    {
        await _repo.DesvincularAsync(ticketId, articuloId);
        return ResultadoOperacion.Ok();
    }
}
