using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Soporte.Abstracciones.DTOs;
using Soporte.Abstracciones.Enumeraciones;
using Soporte.Abstracciones.Interfaces.DA;
using Soporte.Abstracciones.Modelos;
using Soporte.DA.Contexto;

namespace Soporte.DA.Repositorios;

public class TicketRepository : ITicketRepository
{
    private const int Abierto = (int)EstadoTicketTipo.Abierto;
    private const int EnProgreso = (int)EstadoTicketTipo.EnProgreso;

    private readonly SoporteDbContext _contexto;

    public TicketRepository(SoporteDbContext contexto)
    {
        _contexto = contexto;
    }

    public async Task<ResultadoPaginado<TicketResumenDto>> ListarAsync(FiltroTicketsDto filtro, int? alcanceAgenteId, int? alcanceClienteId)
    {
        var consulta = _contexto.Tickets.AsNoTracking();

        if (alcanceClienteId.HasValue)
            consulta = consulta.Where(t => t.ClienteId == alcanceClienteId);
        if (alcanceAgenteId.HasValue)
            consulta = consulta.Where(t => t.AgenteAsignadoId == alcanceAgenteId || t.AgenteAsignadoId == null);

        if (filtro.EstadoId.HasValue)
            consulta = consulta.Where(t => t.EstadoId == filtro.EstadoId);
        if (filtro.PrioridadId.HasValue)
            consulta = consulta.Where(t => t.PrioridadId == filtro.PrioridadId);
        if (filtro.SoloSinAsignar)
            consulta = consulta.Where(t => t.AgenteAsignadoId == null);
        else if (filtro.AgenteId.HasValue)
            consulta = consulta.Where(t => t.AgenteAsignadoId == filtro.AgenteId);

        var ahora = DateTime.UtcNow;

        if (filtro.RequiereAtencion)
        {
            consulta = consulta.Where(t => (t.EstadoId == Abierto || t.EstadoId == EnProgreso)
                                           && (t.AgenteAsignadoId == null
                                               || t.FechaCreacion.AddHours(t.Prioridad!.HorasSla) < ahora));
        }

        if (!string.IsNullOrWhiteSpace(filtro.Texto))
        {
            var texto = filtro.Texto.Trim();
            consulta = consulta.Where(t => t.Asunto.Contains(texto) || t.Cliente!.Nombre.Contains(texto));
        }

        var total = await consulta.CountAsync();

        var ordenada = filtro.OrdenarPorActividad
            ? consulta.OrderByDescending(t => t.FechaActualizacion ?? t.FechaCreacion)
            : consulta.OrderByDescending(t => t.FechaCreacion);

        var elementos = await ordenada
            .ThenByDescending(t => t.Id)
            .Skip((filtro.Pagina - 1) * filtro.TamanoPagina)
            .Take(filtro.TamanoPagina)
            .Select(t => new TicketResumenDto
            {
                Id = t.Id,
                Asunto = t.Asunto,
                Categoria = t.Categoria!.Nombre,
                PrioridadId = t.PrioridadId,
                Prioridad = t.Prioridad!.Nombre,
                EstadoId = t.EstadoId,
                Estado = t.Estado!.Nombre,
                Cliente = t.Cliente!.Nombre,
                AgenteAsignado = t.AgenteAsignado != null ? t.AgenteAsignado.Nombre : null,
                FechaCreacion = t.FechaCreacion,
                FechaActualizacion = t.FechaActualizacion,
                SlaVencido = (t.EstadoId == Abierto || t.EstadoId == EnProgreso)
                             && t.FechaCreacion.AddHours(t.Prioridad!.HorasSla) < ahora,
                EsperaRespuestaCliente = (t.EstadoId == Abierto || t.EstadoId == EnProgreso)
                                         && t.Comentarios
                                             .Where(c => !c.EsNotaInterna)
                                             .OrderByDescending(c => c.FechaCreacion)
                                             .ThenByDescending(c => c.Id)
                                             .Select(c => c.SolicitaInformacion)
                                             .FirstOrDefault()
            })
            .ToListAsync();

        return new ResultadoPaginado<TicketResumenDto>
        {
            Elementos = elementos,
            Total = total,
            Pagina = filtro.Pagina,
            TamanoPagina = filtro.TamanoPagina
        };
    }

    public async Task<Ticket?> ObtenerPorIdAsync(int id)
    {
        return await _contexto.Tickets.FindAsync(id);
    }

    public async Task<TicketDetalleDto?> ObtenerDetalleAsync(int id, bool incluirNotasInternas, int lectorId)
    {
        var ahora = DateTime.UtcNow;

        return await _contexto.Tickets
            .AsNoTracking()
            .AsSplitQuery()
            .Where(t => t.Id == id)
            .Select(t => new TicketDetalleDto
            {
                Id = t.Id,
                Asunto = t.Asunto,
                Descripcion = t.Descripcion,
                CategoriaId = t.CategoriaId,
                Categoria = t.Categoria!.Nombre,
                PrioridadId = t.PrioridadId,
                Prioridad = t.Prioridad!.Nombre,
                EstadoId = t.EstadoId,
                Estado = t.Estado!.Nombre,
                ClienteId = t.ClienteId,
                Cliente = t.Cliente!.Nombre,
                AgenteAsignadoId = t.AgenteAsignadoId,
                AgenteAsignado = t.AgenteAsignado != null ? t.AgenteAsignado.Nombre : null,
                FechaCreacion = t.FechaCreacion,
                FechaActualizacion = t.FechaActualizacion,
                SlaVencido = (t.EstadoId == Abierto || t.EstadoId == EnProgreso)
                             && t.FechaCreacion.AddHours(t.Prioridad!.HorasSla) < ahora,
                EsperaRespuestaCliente = (t.EstadoId == Abierto || t.EstadoId == EnProgreso)
                                         && t.Comentarios
                                             .Where(c => !c.EsNotaInterna)
                                             .OrderByDescending(c => c.FechaCreacion)
                                             .ThenByDescending(c => c.Id)
                                             .Select(c => c.SolicitaInformacion)
                                             .FirstOrDefault(),
                RowVersion = Convert.ToBase64String(t.RowVersion),
                Adjuntos = t.Adjuntos
                    .OrderBy(a => a.FechaSubida)
                    .Select(a => new AdjuntoDto
                    {
                        Id = a.Id,
                        NombreOriginal = a.NombreOriginal,
                        TipoContenido = a.TipoContenido,
                        TamanoBytes = a.TamanoBytes,
                        FechaSubida = a.FechaSubida
                    })
                    .ToList(),
                Comentarios = t.Comentarios
                    .Where(c => incluirNotasInternas || !c.EsNotaInterna)
                    .OrderBy(c => c.FechaCreacion)
                    .Select(c => new ComentarioDto
                    {
                        Id = c.Id,
                        TicketId = c.TicketId,
                        UsuarioId = c.UsuarioId,
                        Autor = c.Usuario!.Nombre,
                        RolAutor = c.Usuario.Rol!.Nombre,
                        Texto = c.Texto,
                        EsNotaInterna = c.EsNotaInterna,
                        SolicitaInformacion = c.SolicitaInformacion,
                        FechaCreacion = c.FechaCreacion,
                        // Los comentarios propios cuentan como leídos.
                        Leido = c.UsuarioId == lectorId || c.Lecturas.Any(l => l.UsuarioId == lectorId),
                        Adjuntos = c.Adjuntos
                            .OrderBy(a => a.FechaSubida)
                            .Select(a => new AdjuntoDto
                            {
                                Id = a.Id,
                                NombreOriginal = a.NombreOriginal,
                                TipoContenido = a.TipoContenido,
                                TamanoBytes = a.TamanoBytes,
                                FechaSubida = a.FechaSubida
                            })
                            .ToList()
                    })
                    .ToList(),
                Historial = t.Historial
                    .OrderBy(h => h.FechaCambio)
                    .Select(h => new HistorialTicketDto
                    {
                        Usuario = h.Usuario!.Nombre,
                        EstadoAnterior = h.EstadoAnterior != null ? h.EstadoAnterior.Nombre : null,
                        EstadoNuevoId = h.EstadoNuevoId,
                        EstadoNuevo = h.EstadoNuevo!.Nombre,
                        FechaCambio = h.FechaCambio
                    })
                    .ToList()
            })
            .SingleOrDefaultAsync();
    }

    public async Task<int> CrearAsync(Ticket ticket)
    {
        _contexto.Tickets.Add(ticket);
        await _contexto.SaveChangesAsync();
        return ticket.Id;
    }

    public async Task<bool> ActualizarAsync(Ticket ticket, byte[] rowVersionEsperado)
    {
        var entrada = _contexto.Entry(ticket);
        if (entrada.State == EntityState.Detached)
            _contexto.Tickets.Update(ticket);

        // EF agrega "WHERE RowVersion = @original" al UPDATE; si no afecta filas, lanza la excepción.
        entrada.Property(t => t.RowVersion).OriginalValue = rowVersionEsperado;

        try
        {
            await _contexto.SaveChangesAsync();
            return true;
        }
        catch (DbUpdateConcurrencyException)
        {
            _contexto.ChangeTracker.Clear();
            return false;
        }
    }

    public async Task<(bool Asignado, int? HistorialId)> TomarTicketAsync(int ticketId, int agenteId)
    {
        var asignado = new SqlParameter("@Asignado", SqlDbType.Bit) { Direction = ParameterDirection.Output };
        var historial = new SqlParameter("@HistorialId", SqlDbType.Int) { Direction = ParameterDirection.Output };

        await _contexto.Database.ExecuteSqlRawAsync(
            "EXEC dbo.sp_TomarTicket @TicketId, @AgenteId, @Asignado OUTPUT, @HistorialId OUTPUT",
            new SqlParameter("@TicketId", ticketId),
            new SqlParameter("@AgenteId", agenteId),
            asignado,
            historial);

        return (asignado.Value is true, historial.Value is int id ? id : null);
    }

    public async Task RegistrarActividadAsync(int ticketId, DateTime fechaUtc)
    {
        await _contexto.Tickets
            .Where(t => t.Id == ticketId)
            .ExecuteUpdateAsync(s => s.SetProperty(t => t.FechaActualizacion, fechaUtc));
    }
}
