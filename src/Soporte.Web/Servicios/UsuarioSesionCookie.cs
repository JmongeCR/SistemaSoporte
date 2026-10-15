using System.Security.Claims;
using Soporte.Abstracciones.Enumeraciones;

namespace Soporte.Web.Servicios;

/// <summary>Lee el usuario de la cookie de sesión creada en el login (HU-01).</summary>
public class UsuarioSesionCookie : IUsuarioSesion
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UsuarioSesionCookie(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private ClaimsPrincipal? Usuario => _httpContextAccessor.HttpContext?.User;

    public bool EstaAutenticado => Usuario?.Identity?.IsAuthenticated == true;

    public int UsuarioId => int.TryParse(Usuario?.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : 0;

    public string Nombre => Usuario?.FindFirstValue(ClaimTypes.Name) ?? string.Empty;

    /// <summary>Sin rol válido se asume Cliente (el de menos privilegios); el API vuelve a validar igualmente.</summary>
    public RolTipo Rol => Enum.TryParse<RolTipo>(Usuario?.FindFirstValue(ClaimTypes.Role), out var rol) ? rol : RolTipo.Cliente;
}
