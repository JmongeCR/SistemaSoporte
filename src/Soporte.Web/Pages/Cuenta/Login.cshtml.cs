using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.RateLimiting;
using Soporte.Abstracciones.DTOs;
using Soporte.Abstracciones.Enumeraciones;
using Soporte.Web.Presentacion;
using Soporte.Web.Servicios;

namespace Soporte.Web.Pages.Cuenta;

/// <summary>HU-01: inicio de sesión (diseño "01 - Login").</summary>
[EnableRateLimiting(PoliticasLimite.Intentos)]
public class LoginModel : PageModel
{
    private readonly IAutenticacionApiServicio _autenticacionApi;
    private readonly IGestorSesion _gestorSesion;
    private readonly IUsuarioSesion _sesion;

    public LoginModel(IAutenticacionApiServicio autenticacionApi, IGestorSesion gestorSesion, IUsuarioSesion sesion)
    {
        _autenticacionApi = autenticacionApi;
        _gestorSesion = gestorSesion;
        _sesion = sesion;
    }

    [BindProperty]
    public LoginSolicitudDto Credenciales { get; set; } = new();

    /// <summary>Página protegida que se intentó abrir antes de iniciar sesión.</summary>
    [BindProperty(SupportsGet = true)]
    public string? ReturnUrl { get; set; }

    public string? MensajeError { get; private set; }

    public IActionResult OnGet()
    {
        return _sesion.EstaAutenticado ? LocalRedirect(Destino(_sesion.Rol)) : Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
            return Page();

        var resultado = await _autenticacionApi.IniciarSesionAsync(Credenciales);
        if (!resultado.Exito)
        {
            MensajeError = resultado.Mensaje;
            return Page();
        }

        await _gestorSesion.IniciarAsync(resultado.Datos!);
        return LocalRedirect(Destino((RolTipo)resultado.Datos!.RolId));
    }

    /// <summary>Solo se vuelve a direcciones locales, para evitar redirecciones abiertas a otros sitios.</summary>
    private string Destino(RolTipo rol) =>
        !string.IsNullOrEmpty(ReturnUrl) && Url.IsLocalUrl(ReturnUrl) ? ReturnUrl : Navegacion.InicioSegunRol(rol);
}
