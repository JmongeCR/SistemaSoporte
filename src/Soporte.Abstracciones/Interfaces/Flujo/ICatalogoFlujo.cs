using Soporte.Abstracciones.DTOs;

namespace Soporte.Abstracciones.Interfaces.Flujo;

public interface ICatalogoFlujo
{
    Task<IReadOnlyList<CatalogoDto>> ListarCategoriasAsync();
    Task<IReadOnlyList<CatalogoDto>> ListarPrioridadesAsync();
    Task<IReadOnlyList<CatalogoDto>> ListarEstadosAsync();
    Task<IReadOnlyList<CatalogoDto>> ListarRolesAsync();

    /// <summary>HU-16: matriz de permisos de solo lectura. Solo Administrador.</summary>
    Task<ResultadoOperacion<MatrizPermisosDto>> ObtenerMatrizPermisosAsync(int rolId, UsuarioContextoDto usuario);

    Task<IReadOnlyList<PrioridadSlaDto>> ListarPrioridadesConSlaAsync();
    Task<ResultadoOperacion<PrioridadSlaDto>> ActualizarSlaAsync(int id, ActualizarSlaDto dto, UsuarioContextoDto usuario);
}
