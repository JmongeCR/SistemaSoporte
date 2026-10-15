using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Soporte.Web.Pages;

/// <summary>Se muestra cuando un usuario con sesión intenta abrir una sección de otro rol.</summary>
public class AccesoDenegadoModel : PageModel
{
    public void OnGet()
    {
        Response.StatusCode = StatusCodes.Status403Forbidden;
    }
}
