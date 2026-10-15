using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Soporte.Web.Servicios;

namespace Soporte.Web.Pages.Cuenta;

/// <summary>HU-03 esc. 1: cierre de sesión. Solo por POST (con token antifalsificación).</summary>
public class SalirModel : PageModel
{
    private readonly IGestorSesion _gestorSesion;

    public SalirModel(IGestorSesion gestorSesion)
    {
        _gestorSesion = gestorSesion;
    }

    public IActionResult OnGet() => RedirectToPage("./Login");

    public async Task<IActionResult> OnPostAsync()
    {
        await _gestorSesion.CerrarAsync();
        return RedirectToPage("./Login");
    }
}
