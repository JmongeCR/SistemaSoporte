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
    Task<CatalogoDto> CrearCategoriaAsync(string nombre);
    Task<ResultadoOperacion<CatalogoDto>> ActualizarCategoriaAsync(int id, string nombre);
    Task<ResultadoOperacion> EliminarCategoriaAsync(int id);
    Task<IReadOnlyList<Permiso>> ListarPermisosConRolesAsync();
    Task<IReadOnlyList<PrioridadSlaDto>> ListarPrioridadesConSlaAsync();
    Task<ResultadoOperacion<PrioridadSlaDto>> ActualizarSlaAsync(int id, int horasSla);
}
