using Microsoft.AspNetCore.Mvc.RazorPages;
using Soporte.Abstracciones.DTOs;
using Soporte.Abstracciones.Enumeraciones;
using Soporte.Web.Servicios;

namespace Soporte.Web.Pages.Dashboard;

/// <summary>
/// HU-12/HU-13 y Dashboard del Cliente. Todo sale de la base (vía API); si una consulta falla,
/// esa sección muestra el error en lugar de un valor engañoso.
/// </summary>
public class IndexModel : PageModel
{
    private const int CantidadLista = 5;

    private readonly IDashboardApiServicio _dashboardApi;
    private readonly ITicketsApiServicio _ticketsApi;
    private readonly INotificacionesApiServicio _notificacionesApi;
    private readonly IUsuarioSesion _sesion;

    public IndexModel(
        IDashboardApiServicio dashboardApi,
        ITicketsApiServicio ticketsApi,
        INotificacionesApiServicio notificacionesApi,
        IUsuarioSesion sesion)
    {
        _dashboardApi = dashboardApi;
        _ticketsApi = ticketsApi;
        _notificacionesApi = notificacionesApi;
        _sesion = sesion;
    }

    public string Nombre => _sesion.Nombre;
    public bool EsCliente => _sesion.Rol == RolTipo.Cliente;
    public bool EsAdministrador => _sesion.Rol == RolTipo.Administrador;

    public ResultadoOperacion<MetricasDashboardDto>? Metricas { get; private set; }
    public ResultadoOperacion<IReadOnlyList<TicketsPorAgenteDto>>? CargaAgentes { get; private set; }
    public ResultadoOperacion<ResumenClienteDto>? ResumenCliente { get; private set; }
    public ResultadoOperacion<ResultadoPaginado<TicketResumenDto>>? RequierenAtencion { get; private set; }
    public ResultadoOperacion<ResultadoPaginado<TicketResumenDto>>? Recientes { get; private set; }
    public int? NotificacionesNoLeidas { get; private set; }

    public async Task OnGetAsync()
    {
        var recientes = _ticketsApi.ListarAsync(new FiltroTicketsDto { OrdenarPorActividad = true, TamanoPagina = CantidadLista });
        var notificaciones = _notificacionesApi.ObtenerResumenAsync();

        if (EsCliente)
        {
            var resumen = _dashboardApi.ObtenerMiResumenAsync();
            await Task.WhenAll(recientes, notificaciones, resumen);
            ResumenCliente = resumen.Result;
        }
        else
        {
            var metricas = _dashboardApi.ObtenerMetricasAsync();
            var atencion = _ticketsApi.ListarAsync(new FiltroTicketsDto { RequiereAtencion = true, OrdenarPorActividad = true, TamanoPagina = CantidadLista });
            var carga = EsAdministrador
                ? _dashboardApi.ObtenerCargaAgentesAsync()
                : Task.FromResult<ResultadoOperacion<IReadOnlyList<TicketsPorAgenteDto>>?>(null)!;
            await Task.WhenAll(recientes, notificaciones, metricas, atencion, carga);
            Metricas = metricas.Result;
            RequierenAtencion = atencion.Result;
            CargaAgentes = carga.Result;
        }

        Recientes = recientes.Result;
        NotificacionesNoLeidas = notificaciones.Result.Exito ? notificaciones.Result.Datos!.NoLeidas : null;
    }
}
