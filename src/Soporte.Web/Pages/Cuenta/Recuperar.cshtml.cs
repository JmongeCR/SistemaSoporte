using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.RateLimiting;
using Soporte.Abstracciones.DTOs;
using Soporte.Web.Presentacion;
using Soporte.Web.Servicios;

namespace Soporte.Web.Pages.Cuenta;

/// <summary>HU-02 esc. 1 y 2: solicitud del enlace de restablecimiento.</summary>
[EnableRateLimiting(PoliticasLimite.Intentos)]
public class RecuperarModel : PageModel
{
    private readonly IAutenticacionApiServicio _autenticacionApi;

    public RecuperarModel(IAutenticacionApiServicio autenticacionApi)
    {
        _autenticacionApi = autenticacionApi;
    }

    [BindProperty]
    public SolicitudRecuperacionDto Solicitud { get; set; } = new();

    public string? MensajeExito { get; private set; }
    public string? MensajeError { get; private set; }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
            return Page();

        var resultado = await _autenticacionApi.SolicitarRecuperacionAsync(Solicitud);
        if (resultado.Exito)
            MensajeExito = resultado.Datos!.Mensaje;
        else
            MensajeError = resultado.Mensaje;

        return Page();
    }
}
