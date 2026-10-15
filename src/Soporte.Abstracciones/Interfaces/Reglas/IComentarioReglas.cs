using Soporte.Abstracciones.DTOs;
using Soporte.Abstracciones.Modelos;

namespace Soporte.Abstracciones.Interfaces.Reglas;

public interface IComentarioReglas
{
    /// <summary>
    /// HU-10: el Cliente no crea notas internas.
    /// HU-08 esc. 3: un ticket Cerrado no acepta respuestas públicas.
    /// </summary>
    ResultadoOperacion ValidarNuevoComentario(Ticket ticket, CrearComentarioDto comentario, UsuarioContextoDto usuario);

    /// <summary>HU-10 esc. 2: indica si el usuario puede ver notas internas.</summary>
    bool PuedeVerNotasInternas(UsuarioContextoDto usuario);
}
