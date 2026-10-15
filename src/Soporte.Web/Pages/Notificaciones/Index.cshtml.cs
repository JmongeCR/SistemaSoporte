using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Soporte.Abstracciones.DTOs;
using Soporte.Web.Presentacion;
using Soporte.Web.Servicios;

namespace Soporte.Web.Pages.Notificaciones;

/// <summary>Centro de notificaciones, panel de la campana y acciones de lectura.</summary>
public class IndexModel : PageModel
{
    private const int CantidadPagina = 50;
    private const int CantidadPanel = 8;

    private readonly INotificacionesApiServicio _notificacionesApi;

    public IndexModel(INotificacionesApiServicio notificacionesApi)
    {
        _notificacionesApi = notificacionesApi;
    }

    public IReadOnlyList<NotificacionDto> Notificaciones { get; private set; } = Array.Empty<NotificacionDto>();
    public string? MensajeError { get; private set; }

    public async Task OnGetAsync()
    {
        var resultado = await _notificacionesApi.ListarAsync(CantidadPagina);
        if (resultado.Exito)
            Notificaciones = resultado.Datos!;
        else
            MensajeError = resultado.Mensaje;
    }

    /// <summary>Contador para la actualización periódica de la campana (sin recargar la página).</summary>
    public async Task<IActionResult> OnGetResumenAsync()
    {
        var resultado = await _notificacionesApi.ObtenerResumenAsync();
        return new JsonResult(new { noLeidas = resultado.Exito ? resultado.Datos!.NoLeidas : (int?)null });
    }

    /// <summary>Contenido del panel desplegable de la campana.</summary>
    public async Task<IActionResult> OnGetPanelAsync()
    {
        var resultado = await _notificacionesApi.ListarAsync(CantidadPanel);
        return Partial("_PanelNotificaciones", new PanelNotificaciones
        {
            Elementos = resultado.Datos ?? Array.Empty<NotificacionDto>(),
            MensajeError = resultado.Exito ? null : resultado.Mensaje,
            VolverA = Uri.TryCreate(Request.Headers.Referer.ToString(), UriKind.Absolute, out var origen) ? origen.PathAndQuery : null
        });
    }

    /// <summary>Marca la notificación como leída y lleva al ticket al que se refiere.</summary>
    public async Task<IActionResult> OnPostAbrirAsync(int id)
    {
        var resultado = await _notificacionesApi.MarcarLeidaAsync(id);
        if (!resultado.Exito)
        {
            TempData["MensajeError"] = resultado.Mensaje;
            return RedirectToPage();
        }

        return RedirectToPage("/Tickets/Detalle", new { id = resultado.Datos!.TicketId });
    }

    public async Task<IActionResult> OnPostMarcarLeidaAsync(int id)
    {
        var resultado = await _notificacionesApi.MarcarLeidaAsync(id);
        if (!resultado.Exito)
            TempData["MensajeError"] = resultado.Mensaje;
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostMarcarTodasAsync(string? volverA)
    {
        var resultado = await _notificacionesApi.MarcarTodasLeidasAsync();
        if (!resultado.Exito)
            TempData["MensajeError"] = resultado.Mensaje;

        return !string.IsNullOrEmpty(volverA) && Url.IsLocalUrl(volverA) ? LocalRedirect(volverA) : RedirectToPage();
    }
}
