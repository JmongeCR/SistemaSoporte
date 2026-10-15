using Microsoft.EntityFrameworkCore;
using Soporte.Abstracciones.Interfaces.DA;
using Soporte.Abstracciones.Modelos;
using Soporte.DA.Contexto;

namespace Soporte.DA.Repositorios;

public class AdjuntoRepository : IAdjuntoRepository
{
    private readonly SoporteDbContext _contexto;

    public AdjuntoRepository(SoporteDbContext contexto)
    {
        _contexto = contexto;
    }

    public async Task<Adjunto?> ObtenerPorIdAsync(int id)
    {
        // Incluye el comentario para conocer su ticket y si es nota interna.
        return await _contexto.Adjuntos
            .AsNoTracking()
            .Include(a => a.Comentario)
            .SingleOrDefaultAsync(a => a.Id == id);
    }

    public async Task<int> CrearAsync(Adjunto adjunto)
    {
        _contexto.Adjuntos.Add(adjunto);
        await _contexto.SaveChangesAsync();
        return adjunto.Id;
    }
}
