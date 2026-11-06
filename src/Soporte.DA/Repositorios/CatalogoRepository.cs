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

    public async Task<CatalogoDto> CrearCategoriaAsync(string nombre)
    {
        var cat = new Categoria { Nombre = nombre.Trim(), Activa = true };
        _contexto.Categorias.Add(cat);
        await _contexto.SaveChangesAsync();
        return new CatalogoDto { Id = cat.Id, Nombre = cat.Nombre };
    }

    public async Task<ResultadoOperacion<CatalogoDto>> ActualizarCategoriaAsync(int id, string nombre)
    {
        var cat = await _contexto.Categorias.FindAsync(id);
        if (cat is null)
            return ResultadoOperacion<CatalogoDto>.NoEncontrado($"No existe la categoría #{id}.");
        cat.Nombre = nombre.Trim();
        await _contexto.SaveChangesAsync();
        return ResultadoOperacion<CatalogoDto>.Ok(new CatalogoDto { Id = cat.Id, Nombre = cat.Nombre });
    }

    public async Task<ResultadoOperacion> EliminarCategoriaAsync(int id)
    {
        var cat = await _contexto.Categorias.FindAsync(id);
        if (cat is null)
            return ResultadoOperacion.NoEncontrado($"No existe la categoría #{id}.");
        var enUso = await _contexto.Tickets.AnyAsync(t => t.CategoriaId == id)
                 || await _contexto.ArticulosKb.AnyAsync(a => a.CategoriaId == id);
        if (enUso)
            return ResultadoOperacion.Fallo("No se puede eliminar una categoría en uso por tickets o artículos.");
        _contexto.Categorias.Remove(cat);
        await _contexto.SaveChangesAsync();
        return ResultadoOperacion.Ok();
    }

    public async Task<IReadOnlyList<Permiso>> ListarPermisosConRolesAsync()
    {
        return await _contexto.Permisos.AsNoTracking()
            .Include(p => p.Roles)
            .OrderBy(p => p.Grupo)
            .ThenBy(p => p.Id)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<PrioridadSlaDto>> ListarPrioridadesConSlaAsync()
    {
        return await _contexto.Prioridades.AsNoTracking()
            .OrderBy(p => p.Id)
            .Select(p => new PrioridadSlaDto { Id = p.Id, Nombre = p.Nombre, HorasSla = p.HorasSla })
            .ToListAsync();
    }

    public async Task<ResultadoOperacion<PrioridadSlaDto>> ActualizarSlaAsync(int id, int horasSla)
    {
        var prioridad = await _contexto.Prioridades.FindAsync(id);
        if (prioridad is null)
            return ResultadoOperacion<PrioridadSlaDto>.NoEncontrado($"No existe la prioridad #{id}.");

        prioridad.HorasSla = horasSla;
        await _contexto.SaveChangesAsync();
        return ResultadoOperacion<PrioridadSlaDto>.Ok(
            new PrioridadSlaDto { Id = prioridad.Id, Nombre = prioridad.Nombre, HorasSla = prioridad.HorasSla });
    }
}
