using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Soporte.Abstracciones.DTOs;
using Soporte.Web.Servicios;

namespace Soporte.Web.Pages.Admin.Usuarios;

public class IndexModel : PageModel
{
    private readonly IUsuariosApiServicio _usuariosApi;

    public IndexModel(IUsuariosApiServicio usuariosApi)
    {
        _usuariosApi = usuariosApi;
    }

    [BindProperty(SupportsGet = true)]
    public FiltroUsuariosDto Filtro { get; set; } = new();

    [BindProperty(SupportsGet = true)]
    public int? UsuarioId { get; set; }

    public IReadOnlyList<UsuarioDto> Usuarios { get; private set; } = Array.Empty<UsuarioDto>();
    public UsuarioDto? UsuarioSeleccionado { get; private set; }
    public MatrizPermisosDto? Permisos { get; private set; }
    public string? MensajeError { get; private set; }

    public async Task OnGetAsync()
    {
        var resultado = await _usuariosApi.ListarAsync(Filtro);
        if (!resultado.Exito)
        {
            MensajeError = resultado.Mensaje ?? "No se pudo cargar la lista de usuarios.";
            return;
        }
        Usuarios = resultado.Datos!;

        if (UsuarioId.HasValue)
        {
            UsuarioSeleccionado = Usuarios.FirstOrDefault(u => u.Id == UsuarioId.Value);
            if (UsuarioSeleccionado is not null)
            {
                var permRes = await _usuariosApi.ObtenerPermisosAsync(UsuarioId.Value);
                if (permRes.Exito) Permisos = permRes.Datos;
            }
        }
    }

    public async Task<IActionResult> OnPostCambiarRolAsync(int usuarioId, int rolId)
    {
        var resultado = await _usuariosApi.CambiarRolAsync(usuarioId, new CambiarRolDto { RolId = rolId });
        if (resultado.Exito)
            TempData["MensajeExito"] = "Rol actualizado.";
        else
            TempData["MensajeError"] = resultado.Mensaje ?? "No se pudo cambiar el rol.";

        return RedirectToPage(new { Filtro.Texto, Filtro.RolId, UsuarioId = usuarioId });
    }

    public async Task<IActionResult> OnPostGuardarPermisosAsync(int usuarioId, List<int>? permisosHabilitados)
    {
        var dto = new ActualizarPermisosUsuarioDto { PermisosHabilitados = permisosHabilitados ?? [] };
        var resultado = await _usuariosApi.ActualizarPermisosAsync(usuarioId, dto);
        if (resultado.Exito)
            TempData["MensajeExito"] = "Permisos actualizados.";
        else
            TempData["MensajeError"] = resultado.Mensaje ?? "No se pudieron guardar los permisos.";

        return RedirectToPage(new { Filtro.Texto, Filtro.RolId, UsuarioId = usuarioId });
    }

    public async Task<IActionResult> OnPostRestablecerPermisosAsync(int usuarioId)
    {
        var resultado = await _usuariosApi.RestablecerPermisosAsync(usuarioId);
        if (resultado.Exito)
            TempData["MensajeExito"] = "Permisos restablecidos al rol por defecto.";
        else
            TempData["MensajeError"] = resultado.Mensaje ?? "No se pudieron restablecer los permisos.";

        return RedirectToPage(new { Filtro.Texto, Filtro.RolId, UsuarioId = usuarioId });
    }
}
