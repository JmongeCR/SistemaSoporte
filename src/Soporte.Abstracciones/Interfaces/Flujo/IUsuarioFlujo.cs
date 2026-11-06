using Soporte.Abstracciones.DTOs;

namespace Soporte.Abstracciones.Interfaces.Flujo;

public interface IUsuarioFlujo
{
    /// <summary>HU-14: lista de usuarios con su rol. Solo Administrador.</summary>
    Task<ResultadoOperacion<IReadOnlyList<UsuarioDto>>> ListarAsync(FiltroUsuariosDto filtro, UsuarioContextoDto usuario);

    /// <summary>Clientes activos para elegir el solicitante de un ticket. Administrador y Agente.</summary>
    Task<ResultadoOperacion<IReadOnlyList<UsuarioDto>>> BuscarClientesAsync(string? texto, UsuarioContextoDto usuario);

    /// <summary>HU-15: cambia el rol, protegiendo al último Administrador. Solo Administrador.</summary>
    Task<ResultadoOperacion<UsuarioDto>> CambiarRolAsync(int usuarioId, CambiarRolDto dto, UsuarioContextoDto usuario);

    /// <summary>Actualiza el nombre y, opcionalmente, la contraseña del usuario autenticado.</summary>
    Task<ResultadoOperacion<UsuarioDto>> ActualizarPerfilAsync(int usuarioId, ActualizarPerfilDto dto, UsuarioContextoDto usuarioActual);

    /// <summary>Permisos efectivos de un usuario (defaults del rol + overrides explícitos). Solo Administrador.</summary>
    Task<ResultadoOperacion<MatrizPermisosDto>> ObtenerPermisosEfectivosAsync(int usuarioId, UsuarioContextoDto usuarioActual);

    /// <summary>Guarda los permisos explícitos del usuario. Solo Administrador.</summary>
    Task<ResultadoOperacion> ActualizarPermisosAsync(int usuarioId, ActualizarPermisosUsuarioDto dto, UsuarioContextoDto usuarioActual);

    /// <summary>Elimina todos los overrides de un usuario, dejándolo con los defaults del rol. Solo Administrador.</summary>
    Task<ResultadoOperacion> RestablecerPermisosAsync(int usuarioId, UsuarioContextoDto usuarioActual);
}
