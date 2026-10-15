using Soporte.Abstracciones.DTOs;
using Soporte.Abstracciones.Interfaces.DA;
using Soporte.Abstracciones.Interfaces.Flujo;
using Soporte.Abstracciones.Interfaces.Reglas;
using Soporte.Abstracciones.Modelos;

namespace Soporte.Flujo.Servicios;

public class ComentarioFlujo : IComentarioFlujo
{
    private readonly ITicketRepository _tickets;
    private readonly IComentarioRepository _comentarios;
    private readonly INotificacionRepository _notificaciones;
    private readonly IUnidadDeTrabajo _unidadDeTrabajo;
    private readonly ITicketReglas _ticketReglas;
    private readonly IComentarioReglas _comentarioReglas;
    private readonly IAdjuntoReglas _adjuntoReglas;
    private readonly INotificacionReglas _notificacionReglas;
    private readonly IAdjuntoFlujo _adjuntoFlujo;

    public ComentarioFlujo(
        ITicketRepository tickets,
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
        _comentarios = comentarios;
        _notificaciones = notificaciones;
        _unidadDeTrabajo = unidadDeTrabajo;
        _ticketReglas = ticketReglas;
        _comentarioReglas = comentarioReglas;
        _adjuntoReglas = adjuntoReglas;
        _notificacionReglas = notificacionReglas;
        _adjuntoFlujo = adjuntoFlujo;
    }

    public async Task<ResultadoOperacion<IReadOnlyList<ComentarioDto>>> ListarAsync(int ticketId, UsuarioContextoDto usuario)
    {
        var ticket = await _tickets.ObtenerPorIdAsync(ticketId);
        if (ticket is null || !_ticketReglas.PuedeVer(ticket, usuario))
            return ResultadoOperacion<IReadOnlyList<ComentarioDto>>.NoEncontrado($"No existe el ticket #{ticketId}.");

        return ResultadoOperacion<IReadOnlyList<ComentarioDto>>.Ok(
            await _comentarios.ListarPorTicketAsync(ticketId, _comentarioReglas.PuedeVerNotasInternas(usuario), usuario.UsuarioId));
    }

    public async Task<ResultadoOperacion<ComentarioDto>> CrearAsync(
        int ticketId, CrearComentarioDto dto, IReadOnlyList<ArchivoEntranteDto> adjuntos, UsuarioContextoDto usuario)
    {
        var ticket = await _tickets.ObtenerPorIdAsync(ticketId);
        if (ticket is null || !_ticketReglas.PuedeVer(ticket, usuario))
            return ResultadoOperacion<ComentarioDto>.NoEncontrado($"No existe el ticket #{ticketId}.");

        var validacion = _comentarioReglas.ValidarNuevoComentario(ticket, dto, usuario);
        if (!validacion.Exito)
            return ResultadoOperacion<ComentarioDto>.Desde(validacion);

        var validacionAdjuntos = _adjuntoReglas.ValidarArchivos(adjuntos);
        if (!validacionAdjuntos.Exito)
            return ResultadoOperacion<ComentarioDto>.Desde(validacionAdjuntos);

        // Comentario, adjuntos, fecha de actividad y notificaciones se confirman juntos.
        return await _unidadDeTrabajo.EjecutarAsync(async () =>
        {
            var ahora = DateTime.UtcNow;
            var comentario = new Comentario
            {
                TicketId = ticketId,
                UsuarioId = usuario.UsuarioId,
                Texto = dto.Texto.Trim(),
                EsNotaInterna = dto.EsNotaInterna,
                SolicitaInformacion = dto.SolicitaInformacion,
                FechaCreacion = ahora
            };
            comentario.Id = await _comentarios.CrearAsync(comentario);

            if (adjuntos.Count > 0)
            {
                var guardado = await _adjuntoFlujo.AgregarAComentarioAsync(ticketId, comentario.Id, adjuntos, usuario);
                if (!guardado.Exito)
                    throw new InvalidOperationException(guardado.Mensaje);   // revierte la transacción
            }

            // Una nota interna no es un avance visible: no cambia la "última actualización" del cliente.
            if (!comentario.EsNotaInterna)
                await _tickets.RegistrarActividadAsync(ticketId, ahora);

            await _notificaciones.CrearAsync(
                NotificacionesMapeo.AEntidades(_notificacionReglas.PorComentario(ticket, comentario, usuario)));

            var creado = (await _comentarios.ListarPorTicketAsync(ticketId, _comentarioReglas.PuedeVerNotasInternas(usuario), usuario.UsuarioId))
                .Single(c => c.Id == comentario.Id);

            var mensaje = comentario.EsNotaInterna ? "Nota interna guardada." : "Respuesta enviada.";
            return ResultadoOperacion<ComentarioDto>.Ok(creado, mensaje);
        });
    }
}
