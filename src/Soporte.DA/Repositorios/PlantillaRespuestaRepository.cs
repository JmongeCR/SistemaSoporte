using Microsoft.EntityFrameworkCore;
using Soporte.Abstracciones.DTOs;
using Soporte.Abstracciones.Interfaces.DA;
using Soporte.Abstracciones.Modelos;
using Soporte.DA.Contexto;

namespace Soporte.DA.Repositorios;

public class PlantillaRespuestaRepository : IPlantillaRespuestaRepository
{
    private readonly SoporteDbContext _db;

    public PlantillaRespuestaRepository(SoporteDbContext db) => _db = db;

    public async Task<IReadOnlyList<PlantillaRespuestaDto>> ListarAsync(bool incluyeInactivas = false)
    {
        return await _db.PlantillasRespuesta
            .Where(p => incluyeInactivas || p.Activa)
            .OrderBy(p => p.Titulo)
            .Select(p => new PlantillaRespuestaDto
            {
                Id     = p.Id,
                Titulo = p.Titulo,
                Texto  = p.Texto,
                Activa = p.Activa
            })
            .ToListAsync();
    }

    public Task<PlantillaRespuesta?> ObtenerAsync(int id) =>
        _db.PlantillasRespuesta.FirstOrDefaultAsync(p => p.Id == id);

    public async Task<PlantillaRespuestaDto> CrearAsync(PlantillaRespuesta plantilla)
    {
        _db.PlantillasRespuesta.Add(plantilla);
        await _db.SaveChangesAsync();
        return new PlantillaRespuestaDto
        {
            Id     = plantilla.Id,
            Titulo = plantilla.Titulo,
            Texto  = plantilla.Texto,
            Activa = plantilla.Activa
        };
    }

    public async Task<PlantillaRespuestaDto?> ActualizarAsync(int id, string titulo, string texto)
    {
        var p = await _db.PlantillasRespuesta.FindAsync(id);
        if (p is null) return null;
        p.Titulo = titulo;
        p.Texto  = texto;
        await _db.SaveChangesAsync();
        return new PlantillaRespuestaDto { Id = p.Id, Titulo = p.Titulo, Texto = p.Texto, Activa = p.Activa };
    }

    public async Task<bool> EliminarAsync(int id)
    {
        var p = await _db.PlantillasRespuesta.FindAsync(id);
        if (p is null) return false;
        p.Activa = false;
        await _db.SaveChangesAsync();
        return true;
    }
}
