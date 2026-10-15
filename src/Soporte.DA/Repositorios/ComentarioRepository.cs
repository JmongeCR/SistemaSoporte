using Microsoft.EntityFrameworkCore;
using Soporte.Abstracciones.DTOs;
using Soporte.Abstracciones.Interfaces.DA;
using Soporte.Abstracciones.Modelos;
using Soporte.DA.Contexto;

namespace Soporte.DA.Repositorios;

public class ComentarioRepository : IComentarioRepository
{
    private readonly SoporteDbContext _contexto;

    public ComentarioRepository(SoporteDbContext contexto)
    {
        _contexto = contexto;
    }

    public async Task<IReadOnlyList<ComentarioDto>> ListarPorTicketAsync(int ticketId, bool incluirNotasInternas, int lectorId)
    {
        return await _contexto.Comentarios
            .AsNoTracking()
            .Where(c => c.TicketId == ticketId && (incluirNotasInternas || !c.EsNotaInterna))
            .OrderBy(c => c.FechaCreacion)
            .Select(c => new ComentarioDto
            {
                Id = c.Id,
                TicketId = c.TicketId,
                UsuarioId = c.UsuarioId,
                Autor = c.Usuario!.Nombre,
                RolAutor = c.Usuario.Rol!.Nombre,
                Texto = c.Texto,
                EsNotaInterna = c.EsNotaInterna,
                SolicitaInformacion = c.SolicitaInformacion,
                FechaCreacion = c.FechaCreacion,
                Leido = c.UsuarioId == lectorId || c.Lecturas.Any(l => l.UsuarioId == lectorId),
                Adjuntos = c.Adjuntos.OrderBy(a => a.FechaSubida).Select(a => new AdjuntoDto
                {
                    Id = a.Id,
                    NombreOriginal = a.NombreOriginal,
                    TipoContenido = a.TipoContenido,
                    TamanoBytes = a.TamanoBytes,
                    FechaSubida = a.FechaSubida
                }).ToList()
            })
            .ToListAsync();
    }

    public async Task<int> CrearAsync(Comentario comentario)
    {
        _contexto.Comentarios.Add(comentario);
        await _contexto.SaveChangesAsync();
        return comentario.Id;
    }

    public async Task MarcarLeidosAsync(int ticketId, int usuarioId, bool incluirNotasInternas)
    {
        var pendientes = await _contexto.Comentarios
            .Where(c => c.TicketId == ticketId
                        && c.UsuarioId != usuarioId
                        && (incluirNotasInternas || !c.EsNotaInterna)
                        && !c.Lecturas.Any(l => l.UsuarioId == usuarioId))
            .Select(c => c.Id)
            .ToListAsync();

        if (pendientes.Count == 0)
            return;

        var ahora = DateTime.UtcNow;
        _contexto.LecturasComentario.AddRange(pendientes.Select(id => new LecturaComentario
        {
            ComentarioId = id,
            UsuarioId = usuarioId,
            FechaLectura = ahora
        }));

        try
        {
            await _contexto.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            // Dos pestañas marcaron lo mismo a la vez (clave primaria repetida): ya quedó registrado.
            _contexto.ChangeTracker.Clear();
        }
    }
}
