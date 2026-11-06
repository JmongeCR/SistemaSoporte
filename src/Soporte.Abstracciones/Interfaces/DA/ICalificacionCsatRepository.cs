using Soporte.Abstracciones.DTOs;
using Soporte.Abstracciones.Modelos;

namespace Soporte.Abstracciones.Interfaces.DA;

public interface ICalificacionCsatRepository
{
    Task<CalificacionCsatDto?> ObtenerPorTicketAsync(int ticketId);
    Task<CalificacionCsatDto> CrearAsync(CalificacionCsat calificacion);
    Task<bool> ExistePorTicketAsync(int ticketId, int clienteId);
}
