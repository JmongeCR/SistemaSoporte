using Soporte.Abstracciones.DTOs;
using Soporte.Abstracciones.Enumeraciones;
using Soporte.Abstracciones.Interfaces.DA;
using Soporte.Abstracciones.Interfaces.Flujo;
using Soporte.Abstracciones.Interfaces.Reglas;
using Soporte.Abstracciones.Modelos;

namespace Soporte.Flujo.Servicios;

public class TicketFlujo : ITicketFlujo
{
    private const string MensajeConflicto =
        "Otro usuario modificó el ticket mientras lo editaba. Recárguelo e intente de nuevo.";

    private readonly ITicketRepository _tickets;
    private readonly ICatalogoRepository _catalogos;
    private readonly IComentarioRepository _comentarios;
    private readonly INotificacionRepository _notificaciones;
    private readonly IUnidadDeTrabajo _unidadDeTrabajo;
    private readonly ITicketReglas _ticketReglas;
    private readonly IComentarioReglas _comentarioReglas;
    private readonly IAdjuntoReglas _adjuntoReglas;
    private readonly INotificacionReglas _notificacionReglas;
    private readonly IAdjuntoFlujo _adjuntoFlujo;

    public TicketFlujo(
        ITicketRepository tickets,
        ICatalogoRepository catalogos,
        IComentarioRepository comentarios,
        INotificacionRepository notificaciones,
        IUnidadDeTrabajo unidadDeTrabajo,
        ITicketReglas ticketReglas,
        IComentarioReglas comentarioReglas,
        IAdjuntoReglas adjuntoReglas,
        INotificacionReglas notificacionReglas,
        IAdjuntoFlujo adjuntoFlujo)
    {
        _tickets = tickets;
        _catalogos = catalogos;
        _comentarios = comentarios;
        _notificaciones = notificaciones;
        _unidadDeTrabajo = unidadDeTrabajo;
        _ticketReglas = ticketReglas;
        _comentarioReglas = comentarioReglas;
        _adjuntoReglas = adjuntoReglas;
        _notificacionReglas = notificacionReglas;
        _adjuntoFlujo = adjuntoFlujo;
    }

    public async Task<ResultadoPaginado<TicketResumenDto>> ListarAsync(FiltroTicketsDto filtro, UsuarioContextoDto usuario)
    {
        // El alcance por rol es el mismo que aplica ITicketReglas.PuedeVer, pero filtrado en la BD.
        int? alcanceAgente = usuario.Rol == RolTipo.Agente ? usuario.UsuarioId : null;
        int? alcanceCliente = usuario.Rol == RolTipo.Cliente ? usuario.UsuarioId : null;

        return await _tickets.ListarAsync(filtro, alcanceAgente, alcanceCliente);
    }

    public async Task<ResultadoOperacion<TicketDetalleDto>> ObtenerAsync(int id, UsuarioContextoDto usuario)
    {
        var ticket = await _tickets.ObtenerPorIdAsync(id);

        // Un ticket ajeno se reporta como inexistente para no revelar que existe.
        if (ticket is null || !_ticketReglas.PuedeVer(ticket, usuario))
            return ResultadoOperacion<TicketDetalleDto>.NoEncontrado($"No existe el ticket #{id}.");

        var detalle = await ObtenerDetalleAsync(id, usuario);

        // HU-09 esc. 3: al abrir el ticket, lo visible para el usuario queda leído
        // (el detalle ya devuelto conserva el estado anterior para resaltar lo nuevo).
        await _comentarios.MarcarLeidosAsync(id, usuario.UsuarioId, _comentarioReglas.PuedeVerNotasInternas(usuario));

        return ResultadoOperacion<TicketDetalleDto>.Ok(detalle);
    }

    public async Task<ResultadoOperacion<TicketDetalleDto>> CrearAsync(
        CrearTicketDto dto, IReadOnlyList<ArchivoEntranteDto> adjuntos, UsuarioContextoDto usuario)
    {
        var permiso = await _ticketReglas.ValidarCreacionAsync(dto, usuario);
        if (!permiso.Exito)
            return ResultadoOperacion<TicketDetalleDto>.Desde(permiso);

        if (!await _catalogos.ExisteCategoriaActivaAsync(dto.CategoriaId))
            return ResultadoOperacion<TicketDetalleDto>.Fallo("La categoría seleccionada no existe.");

        // Se validan antes de crear el ticket: si un archivo es inválido, no se crea nada.
        var validacionAdjuntos = _adjuntoReglas.ValidarArchivos(adjuntos);
        if (!validacionAdjuntos.Exito)
            return ResultadoOperacion<TicketDetalleDto>.Desde(validacionAdjuntos);

        var ahora = DateTime.UtcNow;
        var ticket = new Ticket
        {
            Asunto = dto.Asunto.Trim(),
            Descripcion = dto.Descripcion.Trim(),
            CategoriaId = dto.CategoriaId,
            PrioridadId = dto.PrioridadId,
            EstadoId = (int)EstadoTicketTipo.Abierto,   // HU-04 esc. 1: Abierto y sin agente
            ClienteId = usuario.Rol == RolTipo.Cliente ? usuario.UsuarioId : dto.ClienteId!.Value,
            FechaCreacion = ahora
        };
        // El historial registra quién lo creó (puede ser un agente a nombre del cliente).
        ticket.Historial.Add(new HistorialTicket
        {
            UsuarioId = usuario.UsuarioId,
            EstadoNuevoId = ticket.EstadoId,
            FechaCambio = ahora
        });

        var id = await _tickets.CrearAsync(ticket);

        if (adjuntos.Count > 0)
        {
            var guardado = await _adjuntoFlujo.AgregarATicketAsync(id, adjuntos, usuario);
            if (!guardado.Exito)
                return ResultadoOperacion<TicketDetalleDto>.Desde(guardado);
        }

        return ResultadoOperacion<TicketDetalleDto>.Ok(
            await ObtenerDetalleAsync(id, usuario), $"Ticket #{id} creado correctamente.");
    }

    public async Task<ResultadoOperacion<TicketDetalleDto>> ActualizarAsync(int id, ActualizarTicketDto dto, UsuarioContextoDto usuario)
    {
        var ticket = await _tickets.ObtenerPorIdAsync(id);
        if (ticket is null || !_ticketReglas.PuedeVer(ticket, usuario))
            return ResultadoOperacion<TicketDetalleDto>.NoEncontrado($"No existe el ticket #{id}.");

        if (!TryLeerRowVersion(dto.RowVersion, out var rowVersion))
            return ResultadoOperacion<TicketDetalleDto>.Fallo("La versión del ticket no es válida. Recárguelo e intente de nuevo.");

        // Si el usuario editó una versión vieja, avisar del conflicto antes de evaluar reglas
        // contra un estado que no vio. El RowVersion en SaveChanges cubre las ediciones simultáneas.
        if (!ticket.RowVersion.AsSpan().SequenceEqual(rowVersion))
            return ResultadoOperacion<TicketDetalleDto>.Conflicto(MensajeConflicto);

        var validacion = _ticketReglas.ValidarActualizacion(ticket, dto, usuario);
        if (!validacion.Exito)
            return ResultadoOperacion<TicketDetalleDto>.Desde(validacion);

        if (dto.AgenteAsignadoId is int agenteId && agenteId != ticket.AgenteAsignadoId)
        {
            var agente = await _ticketReglas.ValidarAgenteAsignableAsync(agenteId);
            if (!agente.Exito)
                return ResultadoOperacion<TicketDetalleDto>.Desde(agente);
        }

        if (dto.CategoriaId != ticket.CategoriaId && !await _catalogos.ExisteCategoriaActivaAsync(dto.CategoriaId))
            return ResultadoOperacion<TicketDetalleDto>.Fallo("La categoría seleccionada no existe.");

        var mensaje = dto.MensajePublico?.Trim();
        var cambiaEstado = dto.EstadoId != ticket.EstadoId;
        var cambiaDatos = dto.PrioridadId != ticket.PrioridadId
                          || dto.CategoriaId != ticket.CategoriaId
                          || dto.AgenteAsignadoId != ticket.AgenteAsignadoId;

        // Guardar sin cambios reales no persiste nada ni genera notificaciones.
        if (!cambiaEstado && !cambiaDatos && string.IsNullOrEmpty(mensaje))
            return ResultadoOperacion<TicketDetalleDto>.Ok(await ObtenerDetalleAsync(id, usuario), "No había cambios para guardar.");

        var estadoAnteriorId = ticket.EstadoId;

        return await _unidadDeTrabajo.EjecutarAsync(async () =>
        {
            var ahora = DateTime.UtcNow;

            HistorialTicket? historial = null;
            if (cambiaEstado)
            {
                historial = new HistorialTicket
                {
                    UsuarioId = usuario.UsuarioId,
                    EstadoAnteriorId = estadoAnteriorId,
                    EstadoNuevoId = dto.EstadoId,
                    FechaCambio = ahora
                };
                ticket.Historial.Add(historial);
            }

            // Mensaje público que acompaña el cambio (ej. explicación de la resolución).
            Comentario? comentario = null;
            if (!string.IsNullOrEmpty(mensaje))
            {
                comentario = new Comentario { UsuarioId = usuario.UsuarioId, Texto = mensaje, FechaCreacion = ahora };
                ticket.Comentarios.Add(comentario);
            }

            ticket.EstadoId = dto.EstadoId;
            ticket.PrioridadId = dto.PrioridadId;
            ticket.CategoriaId = dto.CategoriaId;
            ticket.AgenteAsignadoId = dto.AgenteAsignadoId;

            // Prioridad, categoría o agente son datos administrativos: no cuentan como avance visible.
            if (cambiaEstado || comentario is not null)
                ticket.FechaActualizacion = ahora;

            if (!await _tickets.ActualizarAsync(ticket, rowVersion))
                return ResultadoOperacion<TicketDetalleDto>.Conflicto(MensajeConflicto);

            // Un cambio de estado con explicación es un único evento: una sola notificación.
            var pendientes = historial is not null
                ? _notificacionReglas.PorCambioEstado(ticket, estadoAnteriorId, dto.EstadoId, historial.Id, usuario, comentario is not null)
                : comentario is not null
                    ? _notificacionReglas.PorComentario(ticket, comentario, usuario)
                    : Array.Empty<NotificacionPendienteDto>();
            await _notificaciones.CrearAsync(NotificacionesMapeo.AEntidades(pendientes));

            return ResultadoOperacion<TicketDetalleDto>.Ok(await ObtenerDetalleAsync(id, usuario), "Ticket actualizado correctamente.");
        });
    }

    public async Task<ResultadoOperacion> TomarAsync(int id, UsuarioContextoDto usuario)
    {
        var ticket = await _tickets.ObtenerPorIdAsync(id);
        if (ticket is null || !_ticketReglas.PuedeVer(ticket, usuario))
            return ResultadoOperacion.NoEncontrado($"No existe el ticket #{id}.");

        var validacion = _ticketReglas.ValidarTomaTicket(ticket, usuario);
        if (!validacion.Exito)
            return validacion;

        var estadoAnteriorId = ticket.EstadoId;

        return await _unidadDeTrabajo.EjecutarAsync(async () =>
        {
            // La regla anterior valida con datos ya leídos; el SP resuelve la carrera entre dos agentes.
            var (asignado, historialId) = await _tickets.TomarTicketAsync(id, usuario.UsuarioId);
            if (!asignado)
                return ResultadoOperacion.Conflicto("El ticket ya fue asignado a otro agente.");

            // Si pasó de Abierto a En Progreso, el cliente se entera.
            if (historialId is int historial)
            {
                var pendientes = _notificacionReglas.PorCambioEstado(
                    ticket, estadoAnteriorId, (int)EstadoTicketTipo.EnProgreso, historial, usuario, conMensajePublico: false);
                await _notificaciones.CrearAsync(NotificacionesMapeo.AEntidades(pendientes));
            }

            return ResultadoOperacion.Ok($"Ticket #{id} asignado correctamente.");
        });
    }

    private async Task<TicketDetalleDto> ObtenerDetalleAsync(int id, UsuarioContextoDto usuario)
    {
        var detalle = await _tickets.ObtenerDetalleAsync(id, _comentarioReglas.PuedeVerNotasInternas(usuario), usuario.UsuarioId);
        return detalle ?? throw new InvalidOperationException($"El ticket #{id} no se encontró después de guardarlo.");
    }

    private static bool TryLeerRowVersion(string valor, out byte[] rowVersion)
    {
        rowVersion = new byte[8];
        return Convert.TryFromBase64String(valor, rowVersion, out var bytes) && bytes == rowVersion.Length;
    }
}
