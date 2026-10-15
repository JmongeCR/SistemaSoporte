using Microsoft.EntityFrameworkCore;
using Soporte.Abstracciones.DTOs;
using Soporte.Abstracciones.Enumeraciones;
using Soporte.Abstracciones.Interfaces.DA;
using Soporte.DA.Contexto;

namespace Soporte.DA.Repositorios;

public class DashboardRepository : IDashboardRepository
{
    private readonly SoporteDbContext _contexto;

    public DashboardRepository(SoporteDbContext contexto)
    {
        _contexto = contexto;
    }

    public async Task<MetricasDashboardDto> ObtenerMetricasAsync(int? agenteId)
    {
        // El SP siempre devuelve una fila (en cero si no hay tickets, HU-12 esc. 2).
        var filas = await _contexto.Database
            .SqlQuery<MetricasDashboardDto>($"EXEC dbo.sp_ObtenerMetricasDashboard @AgenteId = {agenteId}")
            .ToListAsync();
        return filas.Single();
    }

    public async Task<IReadOnlyList<TicketsPorAgenteDto>> ObtenerTicketsPorAgenteAsync()
    {
        return await _contexto.Database
            .SqlQuery<TicketsPorAgenteDto>($"EXEC dbo.sp_TicketsPorAgente")
            .ToListAsync();
    }

    public async Task<ResumenClienteDto> ObtenerResumenClienteAsync(int clienteId)
    {
        const int abierto = (int)EstadoTicketTipo.Abierto;
        const int enProgreso = (int)EstadoTicketTipo.EnProgreso;

        var tickets = _contexto.Tickets.AsNoTracking().Where(t => t.ClienteId == clienteId);

        var porEstado = await tickets
            .GroupBy(t => t.EstadoId)
            .Select(g => new { EstadoId = g.Key, Cantidad = g.Count() })
            .ToListAsync();

        var esperanRespuesta = await tickets
            .Where(t => t.EstadoId == abierto || t.EstadoId == enProgreso)
            .CountAsync(t => t.Comentarios
                .Where(c => !c.EsNotaInterna)
                .OrderByDescending(c => c.FechaCreacion)
                .ThenByDescending(c => c.Id)
                .Select(c => c.SolicitaInformacion)
                .FirstOrDefault());

        int Contar(EstadoTicketTipo estado) => porEstado.SingleOrDefault(e => e.EstadoId == (int)estado)?.Cantidad ?? 0;

        return new ResumenClienteDto
        {
            Abiertos = Contar(EstadoTicketTipo.Abierto),
            EnProgreso = Contar(EstadoTicketTipo.EnProgreso),
            Resueltos = Contar(EstadoTicketTipo.Resuelto),
            Cerrados = Contar(EstadoTicketTipo.Cerrado),
            EsperanTuRespuesta = esperanRespuesta,
            Total = porEstado.Sum(e => e.Cantidad)
        };
    }
}
