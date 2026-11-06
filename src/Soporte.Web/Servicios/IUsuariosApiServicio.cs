using Soporte.Abstracciones.DTOs;

namespace Soporte.Web.Servicios;

public interface IUsuariosApiServicio
{
    /// <summary>Lista todos los usuarios (solo Administrador).</summary>
    Task<ResultadoOperacion<IReadOnlyList<UsuarioDto>>> ListarAsync(FiltroUsuariosDto filtro);

    /// <summary>Clientes activos para elegir el solicitante de un ticket (Administrador y Agente).</summary>
    Task<ResultadoOperacion<IReadOnlyList<UsuarioDto>>> ListarClientesAsync();

    /// <summary>Agentes para reasignar un ticket (solo Administrador).</summary>
    Task<ResultadoOperacion<IReadOnlyList<UsuarioDto>>> ListarAgentesAsync();

    /// <summary>Actualiza nombre y/o contraseña del usuario autenticado.</summary>
    Task<ResultadoOperacion<UsuarioDto>> ActualizarPerfilAsync(ActualizarPerfilDto dto);

    /// <summary>Cambia el rol de un usuario (solo Administrador).</summary>
    Task<ResultadoOperacion<UsuarioDto>> CambiarRolAsync(int id, CambiarRolDto dto);

    /// <summary>Devuelve la matriz de permisos (con overrides) de un usuario.</summary>
    Task<ResultadoOperacion<MatrizPermisosDto>> ObtenerPermisosAsync(int id);

    /// <summary>Guarda los overrides de permisos del usuario.</summary>
    Task<ResultadoOperacion> ActualizarPermisosAsync(int id, ActualizarPermisosUsuarioDto dto);

    /// <summary>Elimina los overrides y vuelve a los permisos del rol.</summary>
    Task<ResultadoOperacion> RestablecerPermisosAsync(int id);
}
