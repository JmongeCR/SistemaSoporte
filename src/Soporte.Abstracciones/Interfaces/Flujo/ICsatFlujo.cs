using Soporte.Abstracciones.DTOs;

namespace Soporte.Abstracciones.Interfaces.Flujo;

public interface ICsatFlujo
{
    Task<CalificacionCsatDto?> ObtenerPorTicketAsync(int ticketId);
    Task<ResultadoOperacion<CalificacionCsatDto>> CalificarAsync(int ticketId, CrearCsatDto dto, UsuarioContextoDto usuario);
}
