using System.Globalization;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Soporte.Abstracciones.DTOs;
using Soporte.Abstracciones.Enumeraciones;

namespace Soporte.Web.Servicios;

/// <summary>
/// Guarda en la cookie (cifrada por ASP.NET Core) los datos del usuario y el token JWT del API,
/// que <see cref="TokenSesionHandler"/> adjunta a cada llamada.
/// </summary>
public class GestorSesionCookie : IGestorSesion
{
    public const string ClaimToken = "token_api";
    public const string ClaimTokenExpira = "token_api_expira";

    private readonly IHttpContextAccessor _httpContextAccessor;

    public GestorSesionCookie(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private HttpContext Contexto => _httpContextAccessor.HttpContext
        ?? throw new InvalidOperationException("La sesión solo se maneja dentro de una petición HTTP.");

    public async Task IniciarAsync(SesionDto sesion)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, sesion.UsuarioId.ToString()),
            new(ClaimTypes.Name, sesion.Nombre),
            new(ClaimTypes.Email, sesion.Correo),
            new(ClaimTypes.Role, ((RolTipo)sesion.RolId).ToString()),
            new(ClaimToken, sesion.Token),
            new(ClaimTokenExpira, sesion.Expira.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture))
        };

        var identidad = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await Contexto.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identidad));
    }

    public async Task CerrarAsync()
    {
        await Contexto.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    }

    /// <summary>true si el token del API guardado en la sesión ya venció.</summary>
    public static bool TokenVencido(ClaimsPrincipal usuario)
    {
        var valor = usuario.FindFirstValue(ClaimTokenExpira);
        return !DateTime.TryParse(valor, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var expira)
               || expira.ToUniversalTime() <= DateTime.UtcNow;
    }
}
