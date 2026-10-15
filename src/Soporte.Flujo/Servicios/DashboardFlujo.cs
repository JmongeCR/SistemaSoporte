using Soporte.Abstracciones.DTOs;
using Soporte.Abstracciones.Enumeraciones;
using Soporte.Abstracciones.Interfaces.DA;
using Soporte.Abstracciones.Interfaces.Flujo;

namespace Soporte.Flujo.Servicios;

public class DashboardFlujo : IDashboardFlujo
{
    private readonly IDashboardRepository _dashboard;

    public DashboardFlujo(IDashboardRepository dashboard)
    {
        _dashboard = dashboard;
    }

    public async Task<ResultadoOperacion<MetricasDashboardDto>> ObtenerMetricasAsync(UsuarioContextoDto usuario)
    {
        return usuario.Rol switch
        {
            RolTipo.Administrador => ResultadoOperacion<MetricasDashboardDto>.Ok(await _dashboard.ObtenerMetricasAsync(null)),
            RolTipo.Agente => ResultadoOperacion<MetricasDashboardDto>.Ok(await _dashboard.ObtenerMetricasAsync(usuario.UsuarioId)),
            _ => ResultadoOperacion<MetricasDashboardDto>.SinPermiso("Las métricas del equipo son solo para Administradores y Agentes.")
        };
    }

    public async Task<ResultadoOperacion<IReadOnlyList<TicketsPorAgenteDto>>> ObtenerTicketsPorAgenteAsync(UsuarioContextoDto usuario)
    {
        return usuario.Rol == RolTipo.Administrador
            ? ResultadoOperacion<IReadOnlyList<TicketsPorAgenteDto>>.Ok(await _dashboard.ObtenerTicketsPorAgenteAsync())
            : ResultadoOperacion<IReadOnlyList<TicketsPorAgenteDto>>.SinPermiso("La carga por agente es solo para Administradores.");
    }

    public async Task<ResultadoOperacion<ResumenClienteDto>> ObtenerResumenClienteAsync(UsuarioContextoDto usuario)
    {
        // El cliente siempre es el de la sesión: no se acepta un Id enviado por la petición.
        return usuario.Rol == RolTipo.Cliente
            ? ResultadoOperacion<ResumenClienteDto>.Ok(await _dashboard.ObtenerResumenClienteAsync(usuario.UsuarioId))
            : ResultadoOperacion<ResumenClienteDto>.SinPermiso("El resumen de casos propios es para Clientes.");
    }
}
