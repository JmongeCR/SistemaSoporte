using Soporte.Abstracciones.DTOs;

namespace Soporte.Web.Servicios;

public class DashboardApiServicio : IDashboardApiServicio
{
    private readonly IApiSoporteCliente _api;

    public DashboardApiServicio(IApiSoporteCliente api)
    {
        _api = api;
    }

    public async Task<ResultadoOperacion<MetricasDashboardDto>> ObtenerMetricasAsync() =>
        await _api.GetAsync<MetricasDashboardDto>("api/dashboard/metricas");

    public async Task<ResultadoOperacion<IReadOnlyList<TicketsPorAgenteDto>>> ObtenerCargaAgentesAsync() =>
        await _api.GetAsync<IReadOnlyList<TicketsPorAgenteDto>>("api/dashboard/carga-agentes");

    public async Task<ResultadoOperacion<ResumenClienteDto>> ObtenerMiResumenAsync() =>
        await _api.GetAsync<ResumenClienteDto>("api/dashboard/mi-resumen");
}
