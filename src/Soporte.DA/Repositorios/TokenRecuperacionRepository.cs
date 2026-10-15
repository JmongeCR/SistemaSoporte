using Microsoft.EntityFrameworkCore;
using Soporte.Abstracciones.Interfaces.DA;
using Soporte.Abstracciones.Modelos;
using Soporte.DA.Contexto;

namespace Soporte.DA.Repositorios;

public class TokenRecuperacionRepository : ITokenRecuperacionRepository
{
    private readonly SoporteDbContext _contexto;

    public TokenRecuperacionRepository(SoporteDbContext contexto)
    {
        _contexto = contexto;
    }

    public async Task CrearAsync(TokenRecuperacion token)
    {
        _contexto.TokensRecuperacion.Add(token);
        await _contexto.SaveChangesAsync();
    }

    public async Task<TokenRecuperacion?> ObtenerPorHashAsync(string tokenHash)
    {
        return await _contexto.TokensRecuperacion
            .AsNoTracking()
            .SingleOrDefaultAsync(t => t.TokenHash == tokenHash);
    }

    public async Task MarcarUsadoAsync(int id)
    {
        await _contexto.TokensRecuperacion
            .Where(t => t.Id == id)
            .ExecuteUpdateAsync(s => s.SetProperty(t => t.Usado, true));
    }

    public async Task<bool> MarcarUsadoSiDisponibleAsync(int id)
    {
        var filas = await _contexto.TokensRecuperacion
            .Where(t => t.Id == id && !t.Usado)
            .ExecuteUpdateAsync(s => s.SetProperty(t => t.Usado, true));
        return filas > 0;
    }
}
