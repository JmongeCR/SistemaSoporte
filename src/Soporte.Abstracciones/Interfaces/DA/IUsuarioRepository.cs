using Soporte.Abstracciones.DTOs;
using Soporte.Abstracciones.Modelos;

namespace Soporte.Abstracciones.Interfaces.DA;

public interface IUsuarioRepository
{
    Task<IReadOnlyList<UsuarioDto>> ListarAsync(FiltroUsuariosDto filtro);
    Task<Usuario?> ObtenerPorIdAsync(int id);
    Task<Usuario?> ObtenerPorCorreoAsync(string correo);
    Task<int> ContarPorRolAsync(int rolId);
    Task ActualizarRolAsync(int usuarioId, int rolId);
    Task ActualizarPasswordAsync(int usuarioId, string passwordHash);
    Task ActualizarNombreAsync(int usuarioId, string nombre);
    Task<IReadOnlyList<UsuarioPermiso>> ObtenerPermisosAsync(int usuarioId);
    /// <summary>Reemplaza todos los overrides del usuario con la lista provista (vacía = usar solo defaults del rol).</summary>
    Task GuardarPermisosAsync(int usuarioId, IReadOnlyList<UsuarioPermiso> overrides);
}
