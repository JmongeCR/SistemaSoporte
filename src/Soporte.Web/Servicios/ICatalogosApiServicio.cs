using Soporte.Abstracciones.DTOs;

namespace Soporte.Web.Servicios;

public interface ICatalogosApiServicio
{
    Task<ResultadoOperacion<IReadOnlyList<CatalogoDto>>> ListarEstadosAsync();
    Task<ResultadoOperacion<IReadOnlyList<CatalogoDto>>> ListarPrioridadesAsync();
    Task<ResultadoOperacion<IReadOnlyList<CatalogoDto>>> ListarCategoriasAsync();
    Task<ResultadoOperacion<IReadOnlyList<CatalogoDto>>> ListarRolesAsync();

    /// <summary>HU-16: permisos del rol, de solo lectura (solo Administrador).</summary>
    Task<ResultadoOperacion<MatrizPermisosDto>> ObtenerMatrizPermisosAsync(int rolId);
}
