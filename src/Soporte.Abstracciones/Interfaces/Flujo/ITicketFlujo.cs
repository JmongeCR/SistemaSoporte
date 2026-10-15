using Soporte.Abstracciones.DTOs;

namespace Soporte.Abstracciones.Interfaces.Flujo;

public interface ITicketFlujo
{
    Task<ResultadoPaginado<TicketResumenDto>> ListarAsync(FiltroTicketsDto filtro, UsuarioContextoDto usuario);
    Task<ResultadoOperacion<TicketDetalleDto>> ObtenerAsync(int id, UsuarioContextoDto usuario);
    /// <summary>
    /// HU-04. Los adjuntos se validan antes de crear el ticket, para no dejar un ticket
    /// creado con archivos rechazados.
    /// </summary>
    Task<ResultadoOperacion<TicketDetalleDto>> CrearAsync(CrearTicketDto dto, IReadOnlyList<ArchivoEntranteDto> adjuntos, UsuarioContextoDto usuario);
    Task<ResultadoOperacion<TicketDetalleDto>> ActualizarAsync(int id, ActualizarTicketDto dto, UsuarioContextoDto usuario);
    Task<ResultadoOperacion> TomarAsync(int id, UsuarioContextoDto usuario);
}
