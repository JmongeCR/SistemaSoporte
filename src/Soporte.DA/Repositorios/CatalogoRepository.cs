using Microsoft.EntityFrameworkCore;
using Soporte.Abstracciones.DTOs;
using Soporte.Abstracciones.Interfaces.DA;
using Soporte.Abstracciones.Modelos;
using Soporte.DA.Contexto;

namespace Soporte.DA.Repositorios;

public class CatalogoRepository : ICatalogoRepository
{
    private readonly SoporteDbContext _contexto;

    public CatalogoRepository(SoporteDbContext contexto)
    {
        _contexto = contexto;
    }

    public async Task<IReadOnlyList<CatalogoDto>> ListarCategoriasAsync()
    {
        return await _contexto.Categorias.AsNoTracking()
            .Where(c => c.Activa)
            .OrderBy(c => c.Nombre)
            .Select(c => new CatalogoDto { Id = c.Id, Nombre = c.Nombre })
            .ToListAsync();
    }

    public async Task<IReadOnlyList<CatalogoDto>> ListarPrioridadesAsync()
    {
        return await _contexto.Prioridades.AsNoTracking()
            .OrderBy(p => p.Id)
            .Select(p => new CatalogoDto { Id = p.Id, Nombre = p.Nombre })
            .ToListAsync();
    }

    public async Task<IReadOnlyList<CatalogoDto>> ListarEstadosAsync()
    {
        return await _contexto.EstadosTicket.AsNoTracking()
            .OrderBy(e => e.Id)
            .Select(e => new CatalogoDto { Id = e.Id, Nombre = e.Nombre })
            .ToListAsync();
    }

    public async Task<IReadOnlyList<CatalogoDto>> ListarRolesAsync()
    {
        return await _contexto.Roles.AsNoTracking()
            .OrderBy(r => r.Id)
            .Select(r => new CatalogoDto { Id = r.Id, Nombre = r.Nombre })
            .ToListAsync();
    }

    public async Task<bool> ExisteCategoriaActivaAsync(int categoriaId)
    {
        return await _contexto.Categorias.AnyAsync(c => c.Id == categoriaId && c.Activa);
    }

    public async Task<IReadOnlyList<Permiso>> ListarPermisosConRolesAsync()
    {
        return await _contexto.Permisos.AsNoTracking()
            .Include(p => p.Roles)
            .OrderBy(p => p.Grupo)
            .ThenBy(p => p.Id)
            .ToListAsync();
    }
}
