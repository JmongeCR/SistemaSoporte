using Soporte.Abstracciones.DTOs;

namespace Soporte.Abstracciones.Interfaces.Flujo;

public interface IComentarioFlujo
{
    Task<ResultadoOperacion<IReadOnlyList<ComentarioDto>>> ListarAsync(int ticketId, UsuarioContextoDto usuario);
    Task<ResultadoOperacion<ComentarioDto>> CrearAsync(int ticketId, CrearComentarioDto dto, IReadOnlyList<ArchivoEntranteDto> adjuntos, UsuarioContextoDto usuario);
}
