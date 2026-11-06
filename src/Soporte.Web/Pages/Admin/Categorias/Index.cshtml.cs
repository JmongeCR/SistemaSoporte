using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Soporte.Abstracciones.DTOs;
using Soporte.Abstracciones.Enumeraciones;
using Soporte.Web.Servicios;
using System.ComponentModel.DataAnnotations;

namespace Soporte.Web.Pages.Admin.Categorias;

[Authorize]
public class IndexModel : PageModel
{
    private readonly ICatalogosApiServicio _catalogos;
    private readonly IUsuarioSesion _sesion;

    public IndexModel(ICatalogosApiServicio catalogos, IUsuarioSesion sesion)
    {
        _catalogos = catalogos;
        _sesion = sesion;
    }

    public IReadOnlyList<CatalogoDto> Categorias { get; private set; } = [];
    public string? MensajeExito { get; private set; }
    public string? MensajeError { get; private set; }

    [BindProperty]
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [MaxLength(50, ErrorMessage = "Máximo 50 caracteres.")]
    public string NuevaNombre { get; set; } = string.Empty;

    [BindProperty]
    public int EditarId { get; set; }

    [BindProperty]
    [MaxLength(50, ErrorMessage = "Máximo 50 caracteres.")]
    public string EditarNombre { get; set; } = string.Empty;

    private bool EsAdmin => _sesion.EstaAutenticado && _sesion.Rol == RolTipo.Administrador;

    public async Task<IActionResult> OnGetAsync()
    {
        if (!EsAdmin) return Forbid();
        await CargarAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostCrearAsync()
    {
        if (!EsAdmin) return Forbid();
        ModelState.Remove(nameof(EditarNombre));

        if (!ModelState.IsValid)
        {
            await CargarAsync();
            return Page();
        }

        var r = await _catalogos.CrearCategoriaAsync(NuevaNombre);
        if (r.Exito)
            MensajeExito = $"Categoría «{r.Datos!.Nombre}» creada.";
        else
            MensajeError = r.Mensaje;

        await CargarAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostEditarAsync()
    {
        if (!EsAdmin) return Forbid();
        ModelState.Remove(nameof(NuevaNombre));

        if (string.IsNullOrWhiteSpace(EditarNombre))
        {
            ModelState.AddModelError(nameof(EditarNombre), "El nombre es obligatorio.");
            await CargarAsync();
            return Page();
        }

        var r = await _catalogos.ActualizarCategoriaAsync(EditarId, EditarNombre);
        if (r.Exito)
            MensajeExito = "Categoría actualizada.";
        else
            MensajeError = r.Mensaje;

        await CargarAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostEliminarAsync(int id)
    {
        if (!EsAdmin) return Forbid();

        var r = await _catalogos.EliminarCategoriaAsync(id);
        if (r.Exito)
            MensajeExito = "Categoría eliminada.";
        else
            MensajeError = r.Mensaje;

        await CargarAsync();
        return Page();
    }

    private async Task CargarAsync()
    {
        var r = await _catalogos.ListarCategoriasAsync();
        Categorias = r.Exito ? r.Datos! : [];
    }
}
