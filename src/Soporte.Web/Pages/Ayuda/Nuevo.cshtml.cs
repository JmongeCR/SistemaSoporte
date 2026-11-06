using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Soporte.Abstracciones.DTOs;
using Soporte.Abstracciones.Enumeraciones;
using Soporte.Web.Servicios;

namespace Soporte.Web.Pages.Ayuda;

[Authorize]
public class NuevoModel : PageModel
{
    private readonly IBaseConocimientoApiServicio _kb;
    private readonly ICatalogosApiServicio _catalogos;
    private readonly IUsuarioSesion _sesion;

    public NuevoModel(IBaseConocimientoApiServicio kb, ICatalogosApiServicio catalogos, IUsuarioSesion sesion)
    {
        _kb = kb;
        _catalogos = catalogos;
        _sesion = sesion;
    }

    [BindProperty]
    public CrearArticuloKbDto Input { get; set; } = new() { Publicado = true };

    public IEnumerable<SelectListItem> Categorias { get; private set; } = [];

    public async Task<IActionResult> OnGetAsync()
    {
        if (_sesion.Rol == RolTipo.Cliente) return Forbid();
        await CargarCategoriasAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (_sesion.Rol == RolTipo.Cliente) return Forbid();

        if (!ModelState.IsValid)
        {
            await CargarCategoriasAsync();
            return Page();
        }

        var resultado = await _kb.CrearAsync(Input);
        if (!resultado.Exito)
        {
            ModelState.AddModelError(string.Empty, resultado.Mensaje ?? "No se pudo guardar el artículo.");
            await CargarCategoriasAsync();
            return Page();
        }

        return RedirectToPage("/Ayuda/Articulo", new { slug = resultado.Datos!.Slug });
    }

    private async Task CargarCategoriasAsync()
    {
        var resultado = await _catalogos.ListarCategoriasAsync();
        Categorias = resultado.Exito
            ? resultado.Datos!.Select(c => new SelectListItem(c.Nombre, c.Id.ToString()))
            : [];
    }
}
