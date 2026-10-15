using System.Security.Claims;
using Soporte.Abstracciones.DTOs;
using Soporte.Abstracciones.Enumeraciones;

namespace Soporte.API.Servicios;

/// <summary>Lee el usuario de los claims del token JWT validado por el middleware de autenticación.</summary>
public class UsuarioActualProveedor : IUsuarioActualProveedor
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UsuarioActualProveedor(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public UsuarioContextoDto? Obtener()
    {
        var usuario = _httpContextAccessor.HttpContext?.User;
        if (usuario?.Identity?.IsAuthenticated != true)
            return null;

        if (!int.TryParse(usuario.FindFirstValue(ClaimTypes.NameIdentifier), out var id)
            || !Enum.TryParse<RolTipo>(usuario.FindFirstValue(ClaimTypes.Role), out var rol))
            return null;

        return new UsuarioContextoDto { UsuarioId = id, Rol = rol };
    }
}
