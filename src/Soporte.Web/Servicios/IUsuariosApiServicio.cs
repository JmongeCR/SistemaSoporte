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
}
