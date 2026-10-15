using Soporte.Abstracciones.DTOs;
using Soporte.Abstracciones.Modelos;

namespace Soporte.Abstracciones.Interfaces.DA;

public interface ICatalogoRepository
{
    Task<IReadOnlyList<CatalogoDto>> ListarCategoriasAsync();
    Task<IReadOnlyList<CatalogoDto>> ListarPrioridadesAsync();
    Task<IReadOnlyList<CatalogoDto>> ListarEstadosAsync();
    Task<IReadOnlyList<CatalogoDto>> ListarRolesAsync();
    Task<bool> ExisteCategoriaActivaAsync(int categoriaId);
    Task<IReadOnlyList<Permiso>> ListarPermisosConRolesAsync();
}
