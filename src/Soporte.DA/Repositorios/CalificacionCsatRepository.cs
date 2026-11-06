using Microsoft.EntityFrameworkCore;
using Soporte.Abstracciones.DTOs;
using Soporte.Abstracciones.Interfaces.DA;
using Soporte.Abstracciones.Modelos;
using Soporte.DA.Contexto;

namespace Soporte.DA.Repositorios;

public class CalificacionCsatRepository : ICalificacionCsatRepository
{
    private readonly SoporteDbContext _db;

    public CalificacionCsatRepository(SoporteDbContext db) => _db = db;

    public async Task<CalificacionCsatDto?> ObtenerPorTicketAsync(int ticketId)
    {
        return await _db.CalificacionesCsat
            .Where(c => c.TicketId == ticketId)
            .Select(c => new CalificacionCsatDto
            {
                TicketId          = c.TicketId,
                Calificacion      = c.Calificacion,
                Comentario        = c.Comentario,
                FechaCalificacion = c.FechaCalificacion
            })
            .FirstOrDefaultAsync();
    }

    public async Task<CalificacionCsatDto> CrearAsync(CalificacionCsat calificacion)
    {
        _db.CalificacionesCsat.Add(calificacion);
        await _db.SaveChangesAsync();
        return new CalificacionCsatDto
        {
            TicketId          = calificacion.TicketId,
            Calificacion      = calificacion.Calificacion,
            Comentario        = calificacion.Comentario,
            FechaCalificacion = calificacion.FechaCalificacion
        };
    }

    public Task<bool> ExistePorTicketAsync(int ticketId, int clienteId) =>
        _db.CalificacionesCsat.AnyAsync(c => c.TicketId == ticketId && c.ClienteId == clienteId);
}
