using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Soporte.Abstracciones.DTOs;
using Soporte.Web.Presentacion;
using Soporte.Web.Servicios;

namespace Soporte.Web.Pages.Notificaciones;

public class IndexModel : PageModel
{
    private readonly INotificacionesApiServicio _api;
    private readonly IUsuarioSesion _sesion;

    public IndexModel(INotificacionesApiServicio api, IUsuarioSesion sesion)
    {
        _api = api;
        _sesion = sesion;
    }

    public IReadOnlyList<NotificacionDto> Notificaciones { get; private set; } = Array.Empty<NotificacionDto>();
    public int TotalNoLeidas { get; private set; }
    public string? MensajeError { get; private set; }

    public async Task OnGetAsync()
    {
        var resultado = await _api.ListarAsync(50);
        if (!resultado.Exito) { MensajeError = resultado.Mensaje; return; }
        Notificaciones = resultado.Datos!;
        TotalNoLeidas = Notificaciones.Count(n => !n.Leida);
    }

    // Devuelve JSON con el contador de no leídas para la campana (fetch periódico).
    public async Task<IActionResult> OnGetResumenAsync()
    {
        var resultado = await _api.ObtenerResumenAsync();
        if (!resultado.Exito)
            return new JsonResult(new { noLeidas = 0 });
        return new JsonResult(new { noLeidas = resultado.Datos!.NoLeidas });
    }

    // Devuelve HTML parcial del panel desplegable de la campana.
    public async Task<IActionResult> OnGetPanelAsync()
    {
        var resultado = await _api.ListarAsync(10);
        var modelo = new PanelNotificaciones
        {
            Elementos = resultado.Exito ? resultado.Datos! : Array.Empty<NotificacionDto>(),
            MensajeError = resultado.Exito ? null : resultado.Mensaje,
            VolverA = Request.Headers.Referer.ToString()
        };
        return Partial("_PanelNotificaciones", modelo);
    }

    public async Task<IActionResult> OnPostMarcarTodasAsync()
    {
        await _api.MarcarTodasLeidasAsync();
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostAbrirAsync(int id)
    {
        var resultado = await _api.MarcarLeidaAsync(id);
        if (resultado.Exito && resultado.Datos!.TicketId > 0)
            return RedirectToPage("/Tickets/Detalle", new { id = resultado.Datos.TicketId });
        return RedirectToPage();
    }
}
