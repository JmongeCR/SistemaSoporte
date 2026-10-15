using Microsoft.EntityFrameworkCore;
using Soporte.Abstracciones.DTOs;
using Soporte.Abstracciones.Interfaces.DA;
using Soporte.Abstracciones.Modelos;
using Soporte.DA.Contexto;

namespace Soporte.DA.Repositorios;

public class NotificacionRepository : INotificacionRepository
{
    private readonly SoporteDbContext _contexto;

    public NotificacionRepository(SoporteDbContext contexto)
    {
        _contexto = contexto;
    }

    public async Task<int> CrearAsync(IReadOnlyList<Notificacion> notificaciones)
    {
        if (notificaciones.Count == 0)
            return 0;

        // Descarta las repetidas en el lote y las que ya existen (el índice único es la última defensa).
        var unicas = notificaciones
            .GroupBy(n => (n.UsuarioId, n.ClaveEvento))
            .Select(g => g.First())
            .ToList();
        var claves = unicas.Select(n => n.ClaveEvento).Distinct().ToList();
        var usuarios = unicas.Select(n => n.UsuarioId).Distinct().ToList();

        var existentes = (await _contexto.Notificaciones
                .Where(n => usuarios.Contains(n.UsuarioId) && claves.Contains(n.ClaveEvento))
                .Select(n => new { n.UsuarioId, n.ClaveEvento })
                .ToListAsync())
            .Select(n => (n.UsuarioId, n.ClaveEvento))
            .ToHashSet();

        var nuevas = unicas.Where(n => !existentes.Contains((n.UsuarioId, n.ClaveEvento))).ToList();
        if (nuevas.Count == 0)
            return 0;

        _contexto.Notificaciones.AddRange(nuevas);
        await _contexto.SaveChangesAsync();
        return nuevas.Count;
    }

    public async Task<IReadOnlyList<NotificacionDto>> ListarAsync(int usuarioId, bool soloNoLeidas, int cantidad)
    {
        var consulta = _contexto.Notificaciones.AsNoTracking().Where(n => n.UsuarioId == usuarioId);
        if (soloNoLeidas)
            consulta = consulta.Where(n => n.FechaLectura == null);

        return await consulta
            .OrderByDescending(n => n.FechaCreacion)
            .ThenByDescending(n => n.Id)
            .Take(cantidad)
            .Select(n => new NotificacionDto
            {
                Id = n.Id,
                TicketId = n.TicketId,
                TicketAsunto = n.Ticket!.Asunto,
                Tipo = n.Tipo,
                Mensaje = n.Mensaje,
                FechaCreacion = n.FechaCreacion,
                Leida = n.FechaLectura != null
            })
            .ToListAsync();
    }

    public async Task<int> ContarNoLeidasAsync(int usuarioId)
    {
        return await _contexto.Notificaciones.CountAsync(n => n.UsuarioId == usuarioId && n.FechaLectura == null);
    }

    public async Task<NotificacionDto?> MarcarLeidaAsync(int id, int usuarioId)
    {
        // El filtro por UsuarioId impide marcar (o descubrir) notificaciones ajenas.
        await _contexto.Notificaciones
            .Where(n => n.Id == id && n.UsuarioId == usuarioId && n.FechaLectura == null)
            .ExecuteUpdateAsync(s => s.SetProperty(n => n.FechaLectura, DateTime.UtcNow));

        return (await ListarPorIdAsync(id, usuarioId));
    }

    public async Task<int> MarcarTodasLeidasAsync(int usuarioId)
    {
        return await _contexto.Notificaciones
            .Where(n => n.UsuarioId == usuarioId && n.FechaLectura == null)
            .ExecuteUpdateAsync(s => s.SetProperty(n => n.FechaLectura, DateTime.UtcNow));
    }

    private async Task<NotificacionDto?> ListarPorIdAsync(int id, int usuarioId)
    {
        return await _contexto.Notificaciones
            .AsNoTracking()
            .Where(n => n.Id == id && n.UsuarioId == usuarioId)
            .Select(n => new NotificacionDto
            {
                Id = n.Id,
                TicketId = n.TicketId,
                TicketAsunto = n.Ticket!.Asunto,
                Tipo = n.Tipo,
                Mensaje = n.Mensaje,
                FechaCreacion = n.FechaCreacion,
                Leida = n.FechaLectura != null
            })
            .SingleOrDefaultAsync();
    }
}
