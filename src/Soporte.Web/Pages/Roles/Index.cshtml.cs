using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Soporte.Abstracciones.DTOs;
using Soporte.Abstracciones.Enumeraciones;
using Soporte.Web.Servicios;

namespace Soporte.Web.Pages.Roles;

/// <summary>HU-16: matriz de permisos por rol, de solo lectura (diseño 06). Solo Administrador.</summary>
public class IndexModel : PageModel
{
    private readonly ICatalogosApiServicio _catalogosApi;
    private readonly IUsuariosApiServicio _usuariosApi;

    public IndexModel(ICatalogosApiServicio catalogosApi, IUsuariosApiServicio usuariosApi)
    {
        _catalogosApi = catalogosApi;
        _usuariosApi = usuariosApi;
    }

    /// <summary>Rol seleccionado (esc. 3: al elegir otro rol se actualiza el panel).</summary>
    [BindProperty(SupportsGet = true)]
    public int RolId { get; set; } = (int)RolTipo.Administrador;

    public IReadOnlyList<CatalogoDto> Roles { get; private set; } = Array.Empty<CatalogoDto>();

    /// <summary>Cantidad de usuarios por rol; null si no se pudo consultar (no se muestra un conteo falso).</summary>
    public IReadOnlyDictionary<int, int>? UsuariosPorRol { get; private set; }

    public MatrizPermisosDto? Matriz { get; private set; }
    public string? MensajeError { get; private set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var roles = _catalogosApi.ListarRolesAsync();
        var usuarios = _usuariosApi.ListarAsync(new FiltroUsuariosDto());
        var matriz = _catalogosApi.ObtenerMatrizPermisosAsync(RolId);
        await Task.WhenAll(roles, usuarios, matriz);

        Roles = roles.Result.Datos ?? Array.Empty<CatalogoDto>();
        UsuariosPorRol = usuarios.Result.Datos?.GroupBy(u => u.RolId).ToDictionary(g => g.Key, g => g.Count());

        if (matriz.Result.Exito)
            Matriz = matriz.Result.Datos;
        else
            MensajeError = matriz.Result.Mensaje;

        if (matriz.Result.TipoError == TipoError.NoEncontrado)
            Response.StatusCode = StatusCodes.Status404NotFound;
        return Page();
    }
}
