using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Routing;
using Soporte.Abstracciones.DTOs;
using Soporte.Abstracciones.Enumeraciones;
using Soporte.Web.Presentacion;
using Soporte.Web.Servicios;

namespace Soporte.Web.Pages.Roles;

/// <summary>HU-14 (lista, búsqueda y filtro) y HU-15 (cambio de rol) — diseños 06b a 06e. Solo Administrador.</summary>
public class UsuariosModel : PageModel
{
    private readonly IUsuariosApiServicio _usuariosApi;
    private readonly ICatalogosApiServicio _catalogosApi;
    private readonly IUsuarioSesion _sesion;
    private readonly IGestorSesion _gestorSesion;

    public UsuariosModel(IUsuariosApiServicio usuariosApi, ICatalogosApiServicio catalogosApi, IUsuarioSesion sesion, IGestorSesion gestorSesion)
    {
        _usuariosApi = usuariosApi;
        _catalogosApi = catalogosApi;
        _sesion = sesion;
        _gestorSesion = gestorSesion;
    }

    [BindProperty(SupportsGet = true)]
    public FiltroUsuariosDto Filtro { get; set; } = new();

    [BindProperty(SupportsGet = true)]
    public int? UsuarioId { get; set; }

    public IReadOnlyList<UsuarioDto>? Usuarios { get; private set; }
    public IReadOnlyList<CatalogoDto> Roles { get; private set; } = Array.Empty<CatalogoDto>();
    public string? MensajeError { get; private set; }
    public int UsuarioActualId => _sesion.UsuarioId;
    public UsuarioDto? UsuarioSeleccionado { get; private set; }
    public MatrizPermisosDto? PermisosUsuario { get; private set; }

    /// <summary>Texto de ayuda del modal según el rol elegido.</summary>
    public string AvisosPorRolJson => JsonSerializer.Serialize(Roles.ToDictionary(r => r.Id.ToString(), r => DescripcionRoles.De(r.Id).AvisoAlAsignar));

    public IReadOnlyDictionary<int, MatrizPermisosDto> PermisosRoles { get; private set; } = new Dictionary<int, MatrizPermisosDto>();
    public string PermisosRolesJson => JsonSerializer.Serialize(PermisosRoles, new JsonSerializerOptions { PropertyNamingPolicy = null });

    public async Task OnGetAsync()
    {
        if (!ModelState.IsValid)
        {
            ModelState.Clear();
            Filtro = new FiltroUsuariosDto();
        }

        var usuarios = _usuariosApi.ListarAsync(Filtro);
        var roles = _catalogosApi.ListarRolesAsync();
        await Task.WhenAll(usuarios, roles);

        Roles = roles.Result.Datos ?? Array.Empty<CatalogoDto>();
        if (usuarios.Result.Exito)
            Usuarios = usuarios.Result.Datos;
        else
            MensajeError = usuarios.Result.Mensaje;

        if (Roles.Count > 0)
        {
            var tareasPermisos = await Task.WhenAll(Roles.Select(r => _catalogosApi.ObtenerMatrizPermisosAsync(r.Id)));
            PermisosRoles = tareasPermisos
                .Where(r => r.Exito && r.Datos is not null)
                .ToDictionary(r => r.Datos!.RolId, r => r.Datos!);
        }

        if (UsuarioId.HasValue && Usuarios is not null)
        {
            UsuarioSeleccionado = Usuarios.SingleOrDefault(u => u.Id == UsuarioId);
            if (UsuarioSeleccionado is not null)
            {
                var perms = await _usuariosApi.ObtenerPermisosAsync(UsuarioId.Value);
                if (perms.Exito)
                    PermisosUsuario = perms.Datos;
            }
        }
    }

    public async Task<IActionResult> OnPostGuardarPermisosAsync(int usuarioId, [FromForm(Name = "permisosHabilitados")] List<int>? permisosHabilitados)
    {
        var dto = new ActualizarPermisosUsuarioDto { PermisosHabilitados = permisosHabilitados ?? new List<int>() };
        var resultado = await _usuariosApi.ActualizarPermisosAsync(usuarioId, dto);
        if (resultado.Exito)
            TempData["Aviso"] = "Permisos actualizados correctamente.";
        else
        {
            TempData["ErrorTitulo"] = "No se pudieron guardar los permisos.";
            TempData["ErrorDetalle"] = resultado.Mensaje;
        }

        return RedirectToPage(new RouteValueDictionary
        {
            [nameof(UsuarioId)] = usuarioId,
            [$"{nameof(Filtro)}.{nameof(FiltroUsuariosDto.Texto)}"] = Filtro.Texto,
            [$"{nameof(Filtro)}.{nameof(FiltroUsuariosDto.RolId)}"] = Filtro.RolId
        });
    }

    public async Task<IActionResult> OnPostRestablecerPermisosAsync(int usuarioId)
    {
        var resultado = await _usuariosApi.RestablecerPermisosAsync(usuarioId);
        TempData["Aviso"] = resultado.Exito
            ? "Permisos restablecidos al default del rol."
            : resultado.Mensaje ?? "No se pudo restablecer los permisos.";

        return RedirectToPage(new RouteValueDictionary
        {
            [nameof(UsuarioId)] = usuarioId,
            [$"{nameof(Filtro)}.{nameof(FiltroUsuariosDto.Texto)}"] = Filtro.Texto,
            [$"{nameof(Filtro)}.{nameof(FiltroUsuariosDto.RolId)}"] = Filtro.RolId
        });
    }

    /// <param name="usuarioId">Usuario al que se le cambia el rol.</param>
    /// <param name="nuevoRolId">Rol elegido en el modal.</param>
    /// <param name="rolActualId">Rol que tenía al abrir el modal.</param>
    /// <param name="volverTexto">Búsqueda que estaba aplicada, para volver a la misma vista.</param>
    /// <param name="volverRolId">Filtro de rol que estaba aplicado.</param>
    public async Task<IActionResult> OnPostCambiarRolAsync(int usuarioId, int nuevoRolId, int rolActualId, string? volverTexto, int? volverRolId)
    {
        var rolId = nuevoRolId;
        var resultado = await _usuariosApi.CambiarRolAsync(usuarioId, rolId);

        if (!resultado.Exito)
        {
            // HU-15 esc. 2: quitarle el rol a un Administrador solo falla por validación cuando es el último.
            var quitabaAdministrador = rolActualId == (int)RolTipo.Administrador && rolId != (int)RolTipo.Administrador;
            TempData["ErrorTitulo"] = quitabaAdministrador && resultado.TipoError == TipoError.Validacion
                ? "No es posible quitar el rol de Administrador"
                : "No se pudo cambiar el rol";
            TempData["ErrorDetalle"] = resultado.Mensaje;
        }
        else if (usuarioId == _sesion.UsuarioId && resultado.Datos!.RolId != (int)RolTipo.Administrador)
        {
            // Quien se quitó a sí mismo el rol pierde el acceso: se cierra la sesión.
            await _gestorSesion.CerrarAsync();
            TempData["MensajeExito"] = "Cambiaste tu propio rol. Iniciá sesión de nuevo para continuar.";
            return RedirectToPage("/Cuenta/Login");
        }
        else
        {
            TempData["Aviso"] = resultado.Mensaje == "El usuario ya tenía ese rol." ? resultado.Mensaje : "Rol actualizado correctamente";
        }

        // Se vuelve a la misma búsqueda y filtro que había antes de abrir el modal.
        return RedirectToPage(new RouteValueDictionary
        {
            [$"{nameof(Filtro)}.{nameof(FiltroUsuariosDto.Texto)}"] = volverTexto,
            [$"{nameof(Filtro)}.{nameof(FiltroUsuariosDto.RolId)}"] = volverRolId
        });
    }
}
