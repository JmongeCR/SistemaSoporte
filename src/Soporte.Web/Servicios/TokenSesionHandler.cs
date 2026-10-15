using System.Net.Http.Headers;
using System.Security.Claims;

namespace Soporte.Web.Servicios;

/// <summary>Agrega "Authorization: Bearer {token}" a las llamadas al API con el token de la sesión.</summary>
public class TokenSesionHandler : DelegatingHandler
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public TokenSesionHandler(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage solicitud, CancellationToken cancelacion)
    {
        var token = _httpContextAccessor.HttpContext?.User.FindFirstValue(GestorSesionCookie.ClaimToken);
        if (!string.IsNullOrEmpty(token))
            solicitud.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        return base.SendAsync(solicitud, cancelacion);
    }
}
