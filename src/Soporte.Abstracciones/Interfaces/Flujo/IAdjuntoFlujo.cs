using Soporte.Abstracciones.DTOs;

namespace Soporte.Abstracciones.Interfaces.Flujo;

public interface IAdjuntoFlujo
{
    /// <summary>HU-04 esc. 3: adjunta archivos a un ticket visible para el usuario.</summary>
    Task<ResultadoOperacion<IReadOnlyList<AdjuntoDto>>> AgregarATicketAsync(int ticketId, IReadOnlyList<ArchivoEntranteDto> archivos, UsuarioContextoDto usuario);

    /// <summary>HU-11: adjunta archivos a un comentario de un ticket visible para el usuario.</summary>
    Task<ResultadoOperacion<IReadOnlyList<AdjuntoDto>>> AgregarAComentarioAsync(int ticketId, int comentarioId, IReadOnlyList<ArchivoEntranteDto> archivos, UsuarioContextoDto usuario);

    /// <summary>Descarga un adjunto si el usuario puede ver su ticket (y la nota interna, si pertenece a una).</summary>
    Task<ResultadoOperacion<ArchivoDescargaDto>> DescargarAsync(int adjuntoId, UsuarioContextoDto usuario);
}
