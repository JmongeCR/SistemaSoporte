using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Soporte.Abstracciones.DTOs;
using Soporte.Abstracciones.Enumeraciones;
using Soporte.Web.Servicios;

namespace Soporte.Web.Pages.Ayuda;

[Authorize]
public class EditarModel : PageModel
{
    private readonly IBaseConocimientoApiServicio _kb;
    private readonly ICatalogosApiServicio _catalogos;
    private readonly IUsuarioSesion _sesion;

    public EditarModel(IBaseConocimientoApiServicio kb, ICatalogosApiServicio catalogos, IUsuarioSesion sesion)
    {
        _kb = kb;
        _catalogos = catalogos;
        _sesion = sesion;
    }

    [BindProperty]
    public ActualizarArticuloKbDto Input { get; set; } = new();

    public int ArticuloId { get; private set; }
    public string Slug { get; private set; } = string.Empty;
    public IEnumerable<SelectListItem> Categorias { get; private set; } = [];

    public async Task<IActionResult> OnGetAsync(string slug)
    {
        if (_sesion.Rol == RolTipo.Cliente) return Forbid();

        var r = await _kb.ObtenerPorSlugAsync(slug);
        if (!r.Exito || r.Datos is null) return NotFound();

        ArticuloId = r.Datos.Id;
        Slug = r.Datos.Slug;
        Input = new ActualizarArticuloKbDto
        {
            Titulo      = r.Datos.Titulo,
            Contenido   = r.Datos.Contenido,
            CategoriaId = r.Datos.CategoriaId,
            Publicado   = r.Datos.Publicado
        };
        await CargarCategoriasAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(string slug, int articuloId)
    {
        if (_sesion.Rol == RolTipo.Cliente) return Forbid();

        if (!ModelState.IsValid)
        {
            ArticuloId = articuloId;
            Slug = slug;
            await CargarCategoriasAsync();
            return Page();
        }

        var resultado = await _kb.ActualizarAsync(articuloId, Input);
        if (!resultado.Exito)
        {
            ModelState.AddModelError(string.Empty, resultado.Mensaje ?? "No se pudo guardar los cambios.");
            ArticuloId = articuloId;
            Slug = slug;
            await CargarCategoriasAsync();
            return Page();
        }

        return RedirectToPage("/Ayuda/Articulo", new { slug = resultado.Datos!.Slug });
    }

    private async Task CargarCategoriasAsync()
    {
        var r = await _catalogos.ListarCategoriasAsync();
        Categorias = r.Exito
            ? r.Datos!.Select(c => new SelectListItem(c.Nombre, c.Id.ToString()))
            : [];
    }
}
