using Soporte.Abstracciones.DTOs;

namespace Soporte.Web.Servicios;

public interface IUsuariosApiServicio
{
    /// <summary>Clientes activos para elegir el solicitante de un ticket (Administrador y Agente).</summary>
    Task<ResultadoOperacion<IReadOnlyList<UsuarioDto>>> ListarClientesAsync();

    /// <summary>Agentes para reasignar un ticket (solo Administrador).</summary>
    Task<ResultadoOperacion<IReadOnlyList<UsuarioDto>>> ListarAgentesAsync();

    /// <summary>HU-14: usuarios con su rol, con búsqueda y filtro por rol.</summary>
    Task<ResultadoOperacion<IReadOnlyList<UsuarioDto>>> ListarAsync(FiltroUsuariosDto filtro);

    /// <summary>HU-15: cambia el rol (el API protege al último Administrador).</summary>
    Task<ResultadoOperacion<UsuarioDto>> CambiarRolAsync(int usuarioId, int rolId);

    /// <summary>Actualiza nombre y/o contraseña del usuario autenticado.</summary>
    Task<ResultadoOperacion<UsuarioDto>> ActualizarPerfilAsync(ActualizarPerfilDto dto);

    /// <summary>Permisos efectivos del usuario (defaults del rol + overrides). Solo Administrador.</summary>
    Task<ResultadoOperacion<MatrizPermisosDto>> ObtenerPermisosAsync(int usuarioId);

    /// <summary>Guarda los permisos explícitos del usuario. Solo Administrador.</summary>
    Task<ResultadoOperacion> ActualizarPermisosAsync(int usuarioId, ActualizarPermisosUsuarioDto dto);

    /// <summary>Elimina los overrides y deja al usuario con los defaults del rol. Solo Administrador.</summary>
    Task<ResultadoOperacion> RestablecerPermisosAsync(int usuarioId);
}
