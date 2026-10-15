using Microsoft.EntityFrameworkCore;
using Soporte.Abstracciones.DTOs;
using Soporte.Abstracciones.Interfaces.DA;
using Soporte.Abstracciones.Modelos;
using Soporte.DA.Contexto;

namespace Soporte.DA.Repositorios;

public class UsuarioRepository : IUsuarioRepository
{
    private readonly SoporteDbContext _contexto;

    public UsuarioRepository(SoporteDbContext contexto)
    {
        _contexto = contexto;
    }

    public async Task<IReadOnlyList<UsuarioDto>> ListarAsync(FiltroUsuariosDto filtro)
    {
        var consulta = _contexto.Usuarios.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(filtro.Texto))
        {
            var texto = filtro.Texto.Trim();
            consulta = consulta.Where(u => u.Nombre.Contains(texto) || u.Correo.Contains(texto));
        }
        if (filtro.RolId.HasValue)
            consulta = consulta.Where(u => u.RolId == filtro.RolId);

        return await consulta
            .OrderBy(u => u.Nombre)
            .Select(u => new UsuarioDto
            {
                Id = u.Id,
                Nombre = u.Nombre,
                Correo = u.Correo,
                RolId = u.RolId,
                Rol = u.Rol!.Nombre,
                Activo = u.Activo
            })
            .ToListAsync();
    }

    public async Task<Usuario?> ObtenerPorIdAsync(int id)
    {
        return await _contexto.Usuarios.FindAsync(id);
    }

    public async Task<Usuario?> ObtenerPorCorreoAsync(string correo)
    {
        return await _contexto.Usuarios
            .Include(u => u.Rol)
            .SingleOrDefaultAsync(u => u.Correo == correo);
    }

    public async Task<int> ContarPorRolAsync(int rolId)
    {
        return await _contexto.Usuarios.CountAsync(u => u.RolId == rolId && u.Activo);
    }

    public async Task ActualizarRolAsync(int usuarioId, int rolId)
    {
        await _contexto.Usuarios
            .Where(u => u.Id == usuarioId)
            .ExecuteUpdateAsync(s => s.SetProperty(u => u.RolId, rolId));
    }

    public async Task ActualizarPasswordAsync(int usuarioId, string passwordHash)
    {
        await _contexto.Usuarios
            .Where(u => u.Id == usuarioId)
            .ExecuteUpdateAsync(s => s
                .SetProperty(u => u.PasswordHash, passwordHash)
                .SetProperty(u => u.ContrasenaVersion, u => u.ContrasenaVersion + 1));
    }
}
