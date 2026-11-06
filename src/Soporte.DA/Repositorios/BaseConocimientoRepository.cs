using Microsoft.EntityFrameworkCore;
using Soporte.Abstracciones.DTOs;
using Soporte.Abstracciones.Interfaces.DA;
using Soporte.Abstracciones.Modelos;
using Soporte.DA.Contexto;

namespace Soporte.DA.Repositorios;

public class BaseConocimientoRepository : IBaseConocimientoRepository
{
    private readonly SoporteDbContext _contexto;

    public BaseConocimientoRepository(SoporteDbContext contexto)
    {
        _contexto = contexto;
    }

    public async Task<IReadOnlyList<ArticuloKbResumenDto>> ListarAsync(int? categoriaId = null)
    {
        var query = _contexto.ArticulosKb.AsNoTracking()
            .Where(a => a.Publicado);

        if (categoriaId.HasValue)
            query = query.Where(a => a.CategoriaId == categoriaId.Value);

        return await query
            .OrderByDescending(a => a.Vistas)
            .ThenBy(a => a.Titulo)
            .Select(a => new ArticuloKbResumenDto
            {
                Id              = a.Id,
                Titulo          = a.Titulo,
                Slug            = a.Slug,
                CategoriaNombre = a.Categoria.Nombre,
                Vistas          = a.Vistas
            })
            .ToListAsync();
    }

    public async Task<ArticuloKbDto?> ObtenerPorSlugAsync(string slug)
    {
        return await _contexto.ArticulosKb.AsNoTracking()
            .Where(a => a.Slug == slug && a.Publicado)
            .Select(a => new ArticuloKbDto
            {
                Id                = a.Id,
                Titulo            = a.Titulo,
                Contenido         = a.Contenido,
                Slug              = a.Slug,
                CategoriaId       = a.CategoriaId,
                CategoriaNombre   = a.Categoria.Nombre,
                CreadoPorNombre   = a.CreadoPor.Nombre,
                FechaCreacion     = a.FechaCreacion,
                Publicado         = a.Publicado,
                Vistas            = a.Vistas
            })
            .FirstOrDefaultAsync();
    }

    public async Task<ArticuloKbDto?> ObtenerPorIdAsync(int id)
    {
        return await _contexto.ArticulosKb.AsNoTracking()
            .Where(a => a.Id == id && a.Publicado)
            .Select(a => new ArticuloKbDto
            {
                Id                = a.Id,
                Titulo            = a.Titulo,
                Contenido         = a.Contenido,
                Slug              = a.Slug,
                CategoriaId       = a.CategoriaId,
                CategoriaNombre   = a.Categoria.Nombre,
                CreadoPorNombre   = a.CreadoPor.Nombre,
                FechaCreacion     = a.FechaCreacion,
                Publicado         = a.Publicado,
                Vistas            = a.Vistas
            })
            .FirstOrDefaultAsync();
    }

    public async Task<IReadOnlyList<ArticuloKbResumenDto>> ListarPorTicketAsync(int ticketId)
    {
        return await _contexto.TicketsArticulosKb.AsNoTracking()
            .Where(t => t.TicketId == ticketId)
            .Select(t => new ArticuloKbResumenDto
            {
                Id              = t.Articulo.Id,
                Titulo          = t.Articulo.Titulo,
                Slug            = t.Articulo.Slug,
                CategoriaNombre = t.Articulo.Categoria.Nombre,
                Vistas          = t.Articulo.Vistas
            })
            .ToListAsync();
    }

    public async Task VincularAsync(int ticketId, int articuloId, int usuarioId)
    {
        var existe = await _contexto.TicketsArticulosKb
            .AnyAsync(t => t.TicketId == ticketId && t.ArticuloKbId == articuloId);

        if (!existe)
        {
            _contexto.TicketsArticulosKb.Add(new TicketArticuloKb
            {
                TicketId       = ticketId,
                ArticuloKbId   = articuloId,
                VinculadoPorId = usuarioId,
                FechaVinculo   = DateTime.UtcNow
            });
            await _contexto.SaveChangesAsync();
        }
    }

    public async Task DesvincularAsync(int ticketId, int articuloId)
    {
        var vinculo = await _contexto.TicketsArticulosKb
            .FirstOrDefaultAsync(t => t.TicketId == ticketId && t.ArticuloKbId == articuloId);

        if (vinculo is not null)
        {
            _contexto.TicketsArticulosKb.Remove(vinculo);
            await _contexto.SaveChangesAsync();
        }
    }

    public async Task IncrementarVistasAsync(int id)
    {
        await _contexto.ArticulosKb
            .Where(a => a.Id == id)
            .ExecuteUpdateAsync(s => s.SetProperty(a => a.Vistas, a => a.Vistas + 1));
    }
}
