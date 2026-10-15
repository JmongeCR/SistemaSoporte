using Soporte.Abstracciones.DTOs;

namespace Soporte.Web.Servicios;

public interface IDashboardApiServicio
{
    Task<ResultadoOperacion<MetricasDashboardDto>> ObtenerMetricasAsync();
    Task<ResultadoOperacion<IReadOnlyList<TicketsPorAgenteDto>>> ObtenerCargaAgentesAsync();
    Task<ResultadoOperacion<ResumenClienteDto>> ObtenerMiResumenAsync();
}
