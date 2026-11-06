using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Soporte.Abstracciones.DTOs;
using Soporte.Web.Servicios;

namespace Soporte.Web.Pages.Admin.Plantillas;

public class PlantillasModel : PageModel
{
    private readonly IPlantillaApiServicio _api;

    public PlantillasModel(IPlantillaApiServicio api) => _api = api;

    public IReadOnlyList<PlantillaRespuestaDto> Plantillas { get; private set; } = Array.Empty<PlantillaRespuestaDto>();

    [BindProperty]
    public CrearPlantillaDto Dto { get; set; } = new();

    public async Task<IActionResult> OnGetAsync()
    {
        var r = await _api.ListarAsync();
        if (r.Exito) Plantillas = r.Datos!;
        return Page();
    }

    public async Task<IActionResult> OnPostCrearAsync()
    {
        if (!ModelState.IsValid) return await OnGetAsync();
        var r = await _api.CrearAsync(Dto);
        TempData[r.Exito ? "MensajeExito" : "MensajeError"] = r.Exito ? "Plantilla creada." : r.Mensaje;
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostActualizarAsync(int id)
    {
        if (!ModelState.IsValid) return await OnGetAsync();
        var r = await _api.ActualizarAsync(id, Dto);
        TempData[r.Exito ? "MensajeExito" : "MensajeError"] = r.Exito ? "Plantilla actualizada." : r.Mensaje;
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostEliminarAsync(int id)
    {
        var r = await _api.EliminarAsync(id);
        TempData[r.Exito ? "MensajeExito" : "MensajeError"] = r.Exito ? "Plantilla desactivada." : r.Mensaje;
        return RedirectToPage();
    }
}
