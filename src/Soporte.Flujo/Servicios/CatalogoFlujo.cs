using Soporte.Abstracciones.DTOs;
using Soporte.Abstracciones.Enumeraciones;
using Soporte.Abstracciones.Interfaces.DA;
using Soporte.Abstracciones.Interfaces.Flujo;

namespace Soporte.Flujo.Servicios;

public class CatalogoFlujo : ICatalogoFlujo
{
    private readonly ICatalogoRepository _catalogos;

    public CatalogoFlujo(ICatalogoRepository catalogos)
    {
        _catalogos = catalogos;
    }

    public async Task<IReadOnlyList<CatalogoDto>> ListarCategoriasAsync() => await _catalogos.ListarCategoriasAsync();
    public async Task<IReadOnlyList<CatalogoDto>> ListarPrioridadesAsync() => await _catalogos.ListarPrioridadesAsync();
    public async Task<IReadOnlyList<CatalogoDto>> ListarEstadosAsync() => await _catalogos.ListarEstadosAsync();
    public async Task<IReadOnlyList<CatalogoDto>> ListarRolesAsync() => await _catalogos.ListarRolesAsync();
    public async Task<IReadOnlyList<PrioridadSlaDto>> ListarPrioridadesConSlaAsync() => await _catalogos.ListarPrioridadesConSlaAsync();

    public async Task<ResultadoOperacion<PrioridadSlaDto>> ActualizarSlaAsync(int id, ActualizarSlaDto dto, UsuarioContextoDto usuario)
    {
        if (usuario.Rol != RolTipo.Administrador)
            return ResultadoOperacion<PrioridadSlaDto>.SinPermiso("Solo un administrador puede modificar los SLAs.");
        return await _catalogos.ActualizarSlaAsync(id, dto.HorasSla);
    }

    public async Task<ResultadoOperacion<CatalogoDto>> CrearCategoriaAsync(string nombre, UsuarioContextoDto usuario)
    {
        if (usuario.Rol != RolTipo.Administrador)
            return ResultadoOperacion<CatalogoDto>.SinPermiso("Solo administradores pueden crear categorías.");
        var cat = await _catalogos.CrearCategoriaAsync(nombre);
        return ResultadoOperacion<CatalogoDto>.Ok(cat);
    }

    public async Task<ResultadoOperacion<CatalogoDto>> ActualizarCategoriaAsync(int id, string nombre, UsuarioContextoDto usuario)
    {
        if (usuario.Rol != RolTipo.Administrador)
            return ResultadoOperacion<CatalogoDto>.SinPermiso("Solo administradores pueden editar categorías.");
        return await _catalogos.ActualizarCategoriaAsync(id, nombre);
    }

    public async Task<ResultadoOperacion> EliminarCategoriaAsync(int id, UsuarioContextoDto usuario)
    {
        if (usuario.Rol != RolTipo.Administrador)
            return ResultadoOperacion.SinPermiso("Solo administradores pueden eliminar categorías.");
        return await _catalogos.EliminarCategoriaAsync(id);
    }

    public async Task<ResultadoOperacion<MatrizPermisosDto>> ObtenerMatrizPermisosAsync(int rolId, UsuarioContextoDto usuario)
    {
        if (usuario.Rol != RolTipo.Administrador)
            return ResultadoOperacion<MatrizPermisosDto>.SinPermiso("Solo un administrador puede consultar la matriz de permisos.");

        var rol = (await _catalogos.ListarRolesAsync()).SingleOrDefault(r => r.Id == rolId);
        if (rol is null)
            return ResultadoOperacion<MatrizPermisosDto>.NoEncontrado($"No existe el rol #{rolId}.");

        var permisos = await _catalogos.ListarPermisosConRolesAsync();

        // Los grupos salen en el orden en que se sembraron (Visibilidad, Gestión, Comunicación, Administración).
        var grupos = permisos
            .GroupBy(p => p.Grupo)
            .OrderBy(g => g.Min(p => p.Id))
            .Select(g => new GrupoPermisosDto
            {
                Grupo = g.Key,
                Permisos = g.OrderBy(p => p.Id)
                    .Select(p => new PermisoDto
                    {
                        Codigo = p.Codigo,
                        Descripcion = p.Descripcion,
                        Concedido = p.Roles.Any(r => r.RolId == rolId)
                    })
                    .ToList()
            })
            .ToList();

        return ResultadoOperacion<MatrizPermisosDto>.Ok(new MatrizPermisosDto { RolId = rol.Id, Rol = rol.Nombre, Grupos = grupos });
    }
}
