using Soporte.Abstracciones.DTOs;
using Soporte.Abstracciones.Enumeraciones;
using Soporte.Abstracciones.Interfaces.DA;
using Soporte.Abstracciones.Interfaces.Flujo;
using Soporte.Abstracciones.Interfaces.Reglas;
using Soporte.Abstracciones.Modelos;

namespace Soporte.Flujo.Servicios;

public class AdjuntoFlujo : IAdjuntoFlujo
{
    private readonly ITicketRepository _tickets;
    private readonly IAdjuntoRepository _adjuntos;
    private readonly IAlmacenamientoArchivos _almacenamiento;
    private readonly IAdjuntoReglas _adjuntoReglas;
    private readonly ITicketReglas _ticketReglas;
    private readonly IComentarioReglas _comentarioReglas;

    public AdjuntoFlujo(
        ITicketRepository tickets,
        IAdjuntoRepository adjuntos,
        IAlmacenamientoArchivos almacenamiento,
        IAdjuntoReglas adjuntoReglas,
        ITicketReglas ticketReglas,
        IComentarioReglas comentarioReglas)
    {
        _tickets = tickets;
        _adjuntos = adjuntos;
        _almacenamiento = almacenamiento;
        _adjuntoReglas = adjuntoReglas;
        _ticketReglas = ticketReglas;
        _comentarioReglas = comentarioReglas;
    }

    public async Task<ResultadoOperacion<IReadOnlyList<AdjuntoDto>>> AgregarATicketAsync(
        int ticketId, IReadOnlyList<ArchivoEntranteDto> archivos, UsuarioContextoDto usuario)
    {
        var ticket = await _tickets.ObtenerPorIdAsync(ticketId);
        if (ticket is null || !_ticketReglas.PuedeVer(ticket, usuario))
            return ResultadoOperacion<IReadOnlyList<AdjuntoDto>>.NoEncontrado($"No existe el ticket #{ticketId}.");

        if (ticket.EstadoId == (int)EstadoTicketTipo.Cerrado)
            return ResultadoOperacion<IReadOnlyList<AdjuntoDto>>.Fallo("El ticket está cerrado y no admite nuevos adjuntos.");

        return await GuardarAsync(archivos, usuario, ticketId: ticketId, comentarioId: null);
    }

    public async Task<ResultadoOperacion<IReadOnlyList<AdjuntoDto>>> AgregarAComentarioAsync(
        int ticketId, int comentarioId, IReadOnlyList<ArchivoEntranteDto> archivos, UsuarioContextoDto usuario)
    {
        var ticket = await _tickets.ObtenerPorIdAsync(ticketId);
        if (ticket is null || !_ticketReglas.PuedeVer(ticket, usuario))
            return ResultadoOperacion<IReadOnlyList<AdjuntoDto>>.NoEncontrado($"No existe el ticket #{ticketId}.");

        // El estado del ticket ya lo validó la regla del comentario (una nota interna sí se admite en un ticket cerrado).
        return await GuardarAsync(archivos, usuario, ticketId: null, comentarioId: comentarioId);
    }

    private async Task<ResultadoOperacion<IReadOnlyList<AdjuntoDto>>> GuardarAsync(
        IReadOnlyList<ArchivoEntranteDto> archivos, UsuarioContextoDto usuario, int? ticketId, int? comentarioId)
    {
        if (archivos.Count == 0)
            return ResultadoOperacion<IReadOnlyList<AdjuntoDto>>.Fallo("Seleccione al menos un archivo.");

        var validacion = _adjuntoReglas.ValidarArchivos(archivos);
        if (!validacion.Exito)
            return ResultadoOperacion<IReadOnlyList<AdjuntoDto>>.Desde(validacion);

        var guardados = new List<AdjuntoDto>();
        foreach (var archivo in archivos)
        {
            var adjunto = new Adjunto
            {
                TicketId = ticketId,
                ComentarioId = comentarioId,
                UsuarioId = usuario.UsuarioId,
                NombreOriginal = Path.GetFileName(archivo.NombreOriginal),
                TipoContenido = archivo.TipoContenido,
                TamanoBytes = archivo.TamanoBytes,
                RutaAlmacenamiento = await _almacenamiento.GuardarAsync(archivo.Contenido, archivo.NombreOriginal),
                FechaSubida = DateTime.UtcNow
            };
            adjunto.Id = await _adjuntos.CrearAsync(adjunto);

            guardados.Add(new AdjuntoDto
            {
                Id = adjunto.Id,
                NombreOriginal = adjunto.NombreOriginal,
                TipoContenido = adjunto.TipoContenido,
                TamanoBytes = adjunto.TamanoBytes,
                FechaSubida = adjunto.FechaSubida
            });
        }

        return ResultadoOperacion<IReadOnlyList<AdjuntoDto>>.Ok(guardados, $"{guardados.Count} archivo(s) adjuntado(s).");
    }

    public async Task<ResultadoOperacion<ArchivoDescargaDto>> DescargarAsync(int adjuntoId, UsuarioContextoDto usuario)
    {
        var noEncontrado = ResultadoOperacion<ArchivoDescargaDto>.NoEncontrado($"No existe el adjunto #{adjuntoId}.");

        var adjunto = await _adjuntos.ObtenerPorIdAsync(adjuntoId);
        var ticketId = adjunto?.TicketId ?? adjunto?.Comentario?.TicketId;
        if (adjunto is null || ticketId is null)
            return noEncontrado;

        var ticket = await _tickets.ObtenerPorIdAsync(ticketId.Value);
        if (ticket is null || !_ticketReglas.PuedeVer(ticket, usuario))
            return noEncontrado;

        // Un adjunto de nota interna no existe para el cliente (HU-10 esc. 2).
        if (adjunto.Comentario is { EsNotaInterna: true } && !_comentarioReglas.PuedeVerNotasInternas(usuario))
            return noEncontrado;

        return ResultadoOperacion<ArchivoDescargaDto>.Ok(new ArchivoDescargaDto
        {
            NombreOriginal = adjunto.NombreOriginal,
            TipoContenido = adjunto.TipoContenido,
            Contenido = await _almacenamiento.AbrirAsync(adjunto.RutaAlmacenamiento)
        });
    }
}
