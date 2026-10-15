using Soporte.Abstracciones.DTOs;
using Soporte.Abstracciones.Modelos;

namespace Soporte.Abstracciones.Interfaces.DA;

public interface IComentarioRepository
{
    Task<IReadOnlyList<ComentarioDto>> ListarPorTicketAsync(int ticketId, bool incluirNotasInternas, int lectorId);
    Task<int> CrearAsync(Comentario comentario);

    /// <summary>HU-09 esc. 3: registra como leídos los comentarios visibles que el usuario aún no ha visto.</summary>
    Task MarcarLeidosAsync(int ticketId, int usuarioId, bool incluirNotasInternas);
}
