using Microsoft.AspNetCore.Mvc;
using Soporte.Web.Servicios;

namespace Soporte.Web.ViewComponents;

/// <summary>Campana de notificaciones del menú: muestra el contador de no leídas al cargar la página.</summary>
public class CampanaViewComponent : ViewComponent
{
    private readonly INotificacionesApiServicio _notificacionesApi;

    public CampanaViewComponent(INotificacionesApiServicio notificacionesApi)
    {
        _notificacionesApi = notificacionesApi;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        var resumen = await _notificacionesApi.ObtenerResumenAsync();
        // Si el API no responde, no se muestra un contador engañoso (queda oculto hasta la próxima consulta).
        return View(resumen.Exito ? resumen.Datos!.NoLeidas : (int?)null);
    }
}
