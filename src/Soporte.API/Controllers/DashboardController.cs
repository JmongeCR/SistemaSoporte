using Microsoft.AspNetCore.Mvc;
using Soporte.Abstracciones.DTOs;
using Soporte.Abstracciones.Interfaces.Flujo;
using Soporte.API.Servicios;

namespace Soporte.API.Controllers;

/// <summary>Métricas del Dashboard según el rol (HU-12, HU-13) y resumen del Cliente.</summary>
[ApiController]
[Route("api/dashboard")]
[Produces("application/json")]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
public class DashboardController : SoporteControllerBase
{
    private readonly IDashboardFlujo _dashboardFlujo;
    private readonly IUsuarioActualProveedor _usuarioActual;

    public DashboardController(IDashboardFlujo dashboardFlujo, IUsuarioActualProveedor usuarioActual)
    {
        _dashboardFlujo = dashboardFlujo;
        _usuarioActual = usuarioActual;
    }

    /// <summary>Abiertos, en progreso, resueltos hoy y SLA vencido (sp_ObtenerMetricasDashboard). Administrador: total; Agente: su ámbito.</summary>
    [HttpGet("metricas")]
    [ProducesResponseType(typeof(MetricasDashboardDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<MetricasDashboardDto>> ObtenerMetricasAsync()
    {
        var usuario = _usuarioActual.Obtener();
        if (usuario is null)
            return NoAutenticado();

        var resultado = await _dashboardFlujo.ObtenerMetricasAsync(usuario);
        return resultado.Exito ? Ok(resultado.Datos) : RespuestaError(resultado);
    }

    /// <summary>Carga de trabajo por agente (sp_TicketsPorAgente). Solo Administrador.</summary>
    [HttpGet("carga-agentes")]
    [ProducesResponseType(typeof(IReadOnlyList<TicketsPorAgenteDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<TicketsPorAgenteDto>>> ObtenerCargaAgentesAsync()
    {
        var usuario = _usuarioActual.Obtener();
        if (usuario is null)
            return NoAutenticado();

        var resultado = await _dashboardFlujo.ObtenerTicketsPorAgenteAsync(usuario);
        return resultado.Exito ? Ok(resultado.Datos) : RespuestaError(resultado);
    }

    /// <summary>Resumen de los casos propios del Cliente autenticado.</summary>
    [HttpGet("mi-resumen")]
    [ProducesResponseType(typeof(ResumenClienteDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<ResumenClienteDto>> ObtenerMiResumenAsync()
    {
        var usuario = _usuarioActual.Obtener();
        if (usuario is null)
            return NoAutenticado();

        var resultado = await _dashboardFlujo.ObtenerResumenClienteAsync(usuario);
        return resultado.Exito ? Ok(resultado.Datos) : RespuestaError(resultado);
    }
}
