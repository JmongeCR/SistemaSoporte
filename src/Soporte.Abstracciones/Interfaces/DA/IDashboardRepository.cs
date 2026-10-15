using Soporte.Abstracciones.DTOs;

namespace Soporte.Abstracciones.Interfaces.DA;

/// <summary>Consultas de métricas; las del equipo se resuelven con procedimientos almacenados.</summary>
public interface IDashboardRepository
{
    /// <param name="agenteId">Null = total general (HU-12); con valor = ámbito del agente (HU-13).</param>
    Task<MetricasDashboardDto> ObtenerMetricasAsync(int? agenteId);
    Task<IReadOnlyList<TicketsPorAgenteDto>> ObtenerTicketsPorAgenteAsync();

    /// <summary>Conteo de los casos de un cliente por estado.</summary>
    Task<ResumenClienteDto> ObtenerResumenClienteAsync(int clienteId);
}
