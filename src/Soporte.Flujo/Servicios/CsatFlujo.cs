using Soporte.Abstracciones.DTOs;
using Soporte.Abstracciones.Enumeraciones;
using Soporte.Abstracciones.Interfaces.DA;
using Soporte.Abstracciones.Interfaces.Flujo;
using Soporte.Abstracciones.Modelos;

namespace Soporte.Flujo.Servicios;

public class CsatFlujo : ICsatFlujo
{
    private readonly ICalificacionCsatRepository _csatRepo;
    private readonly ITicketRepository _ticketRepo;

    public CsatFlujo(ICalificacionCsatRepository csatRepo, ITicketRepository ticketRepo)
    {
        _csatRepo   = csatRepo;
        _ticketRepo = ticketRepo;
    }

    public Task<CalificacionCsatDto?> ObtenerPorTicketAsync(int ticketId) =>
        _csatRepo.ObtenerPorTicketAsync(ticketId);

    public async Task<ResultadoOperacion<CalificacionCsatDto>> CalificarAsync(int ticketId, CrearCsatDto dto, UsuarioContextoDto usuario)
    {
        if (usuario.Rol != RolTipo.Cliente)
            return ResultadoOperacion<CalificacionCsatDto>.SinPermiso("Solo el cliente puede calificar la atención.");

        var ticket = await _ticketRepo.ObtenerPorIdAsync(ticketId);
        if (ticket is null)
            return ResultadoOperacion<CalificacionCsatDto>.NoEncontrado("Ticket no encontrado.");

        if (ticket.EstadoId is not ((int)EstadoTicketTipo.Resuelto) and not ((int)EstadoTicketTipo.Cerrado))
            return ResultadoOperacion<CalificacionCsatDto>.Fallo("Solo se puede calificar tickets resueltos o cerrados.");

        if (await _csatRepo.ExistePorTicketAsync(ticketId, usuario.UsuarioId))
            return ResultadoOperacion<CalificacionCsatDto>.Fallo("Este ticket ya fue calificado.");

        var calificacion = new CalificacionCsat
        {
            TicketId          = ticketId,
            ClienteId         = usuario.UsuarioId,
            Calificacion      = dto.Calificacion,
            Comentario        = dto.Comentario,
            FechaCalificacion = DateTime.UtcNow
        };
        var resultado = await _csatRepo.CrearAsync(calificacion);
        return ResultadoOperacion<CalificacionCsatDto>.Ok(resultado);
    }
}
