using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Soporte.Abstracciones.DTOs;
using Soporte.Web.Servicios;

namespace Soporte.Web.Pages.Cuenta;

public class PerfilModel : PageModel
{
    private readonly IUsuariosApiServicio _usuariosApi;
    private readonly IUsuarioSesion _sesion;

    public PerfilModel(IUsuariosApiServicio usuariosApi, IUsuarioSesion sesion)
    {
        _usuariosApi = usuariosApi;
        _sesion = sesion;
    }

    [BindProperty]
    public ActualizarPerfilDto Perfil { get; set; } = new();

    public string NombreActual => _sesion.Nombre;
    public string CorreoActual => _sesion.Correo;
    public string RolActual => _sesion.Rol.ToString();

    public string? MensajeExito { get; private set; }
    public string? MensajeError { get; private set; }

    public void OnGet()
    {
        Perfil = new ActualizarPerfilDto { Nombre = _sesion.Nombre };
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
            return Page();

        var resultado = await _usuariosApi.ActualizarPerfilAsync(Perfil);

        if (!resultado.Exito)
        {
            MensajeError = resultado.Mensaje;
            return Page();
        }

        MensajeExito = resultado.Mensaje ?? "Perfil actualizado.";
        Perfil = new ActualizarPerfilDto { Nombre = resultado.Datos!.Nombre };
        return Page();
    }
}
