using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Soporte.Abstracciones.DTOs;
using Soporte.Abstracciones.Enumeraciones;
using Soporte.Web.Servicios;

namespace Soporte.Web.Pages.Admin.Configuracion;

public class SlaModel : PageModel
{
    private readonly ICatalogosApiServicio _catalogos;
    private readonly IUsuarioSesion _sesion;

    public SlaModel(ICatalogosApiServicio catalogos, IUsuarioSesion sesion)
    {
        _catalogos = catalogos;
        _sesion = sesion;
    }

    public IReadOnlyList<PrioridadSlaDto> Prioridades { get; private set; } = Array.Empty<PrioridadSlaDto>();

    public async Task<IActionResult> OnGetAsync()
    {
        if (_sesion.Rol != RolTipo.Administrador)
            return Forbid();

        var resultado = await _catalogos.ListarPrioridadesConSlaAsync();
        if (resultado.Exito) Prioridades = resultado.Datos!;
        return Page();
    }

    public async Task<IActionResult> OnPostActualizarAsync(int id, int horasSla)
    {
        if (_sesion.Rol != RolTipo.Administrador)
            return Forbid();

        var resultado = await _catalogos.ActualizarSlaAsync(id, new ActualizarSlaDto { HorasSla = horasSla });
        if (resultado.Exito)
            TempData["MensajeExito"] = $"SLA de {resultado.Datos!.Nombre} actualizado a {resultado.Datos.HorasSla} horas.";
        else
            TempData["MensajeError"] = resultado.Mensaje ?? "No se pudo actualizar el SLA.";

        return RedirectToPage();
    }

    public static string ClasePrioridad(int id) => id switch
    {
        1 => "pill-cerrado",
        2 => "pill-progreso",
        3 => "pill-abierto",
        4 => "pill-alerta",
        _ => ""
    };
}
