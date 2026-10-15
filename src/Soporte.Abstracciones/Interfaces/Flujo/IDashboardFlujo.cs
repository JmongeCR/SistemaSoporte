using Soporte.Abstracciones.DTOs;

namespace Soporte.Abstracciones.Interfaces.Flujo;

public interface IDashboardFlujo
{
    /// <summary>HU-12/HU-13: Administrador ve el total; Agente, su ámbito. El Cliente no tiene acceso.</summary>
    Task<ResultadoOperacion<MetricasDashboardDto>> ObtenerMetricasAsync(UsuarioContextoDto usuario);

    /// <summary>Carga de trabajo por agente. Solo Administrador.</summary>
    Task<ResultadoOperacion<IReadOnlyList<TicketsPorAgenteDto>>> ObtenerTicketsPorAgenteAsync(UsuarioContextoDto usuario);

    /// <summary>Resumen de los casos propios del Cliente autenticado.</summary>
    Task<ResultadoOperacion<ResumenClienteDto>> ObtenerResumenClienteAsync(UsuarioContextoDto usuario);
}
