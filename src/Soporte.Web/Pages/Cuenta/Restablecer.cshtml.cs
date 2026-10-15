using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Soporte.Abstracciones.DTOs;
using Soporte.Web.Servicios;

namespace Soporte.Web.Pages.Cuenta;

/// <summary>HU-02 esc. 3: nueva contraseña con el enlace recibido por correo (vence en 24 horas).</summary>
public class RestablecerModel : PageModel
{
    private readonly IAutenticacionApiServicio _autenticacionApi;

    public RestablecerModel(IAutenticacionApiServicio autenticacionApi)
    {
        _autenticacionApi = autenticacionApi;
    }

    [BindProperty]
    public RestablecerContrasenaDto Datos { get; set; } = new();

    public string? MensajeError { get; private set; }

    public void OnGet(string? token)
    {
        Datos.Token = token ?? string.Empty;
        if (string.IsNullOrWhiteSpace(token))
            MensajeError = "El enlace está incompleto. Solicitá uno nuevo.";
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
            return Page();

        var resultado = await _autenticacionApi.RestablecerContrasenaAsync(Datos);
        if (!resultado.Exito)
        {
            MensajeError = resultado.Mensaje;
            return Page();
        }

        TempData["MensajeExito"] = resultado.Datos!.Mensaje;
        return RedirectToPage("./Login");
    }
}
