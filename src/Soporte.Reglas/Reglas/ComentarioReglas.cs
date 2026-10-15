using Soporte.Abstracciones.DTOs;
using Soporte.Abstracciones.Enumeraciones;
using Soporte.Abstracciones.Interfaces.Reglas;
using Soporte.Abstracciones.Modelos;

namespace Soporte.Reglas.Reglas;

public class ComentarioReglas : IComentarioReglas
{
    public ResultadoOperacion ValidarNuevoComentario(Ticket ticket, CrearComentarioDto comentario, UsuarioContextoDto usuario)
    {
        if (string.IsNullOrWhiteSpace(comentario.Texto))
            return ResultadoOperacion.Fallo("El comentario no puede estar vacío.");

        if (comentario.EsNotaInterna && !PuedeVerNotasInternas(usuario))
            return ResultadoOperacion.SinPermiso("Solo el equipo de soporte puede agregar notas internas.");

        if (comentario.SolicitaInformacion && !PuedeVerNotasInternas(usuario))
            return ResultadoOperacion.SinPermiso("Solo el equipo de soporte puede solicitar información adicional.");

        if (comentario.SolicitaInformacion && comentario.EsNotaInterna)
            return ResultadoOperacion.Fallo("Una solicitud de información debe ser visible para el cliente, no una nota interna.");

        if (ticket.EstadoId == (int)EstadoTicketTipo.Cerrado && !comentario.EsNotaInterna)
            return ResultadoOperacion.Fallo("El ticket está cerrado y no admite nuevas respuestas públicas.");

        return ResultadoOperacion.Ok();
    }

    public bool PuedeVerNotasInternas(UsuarioContextoDto usuario)
    {
        return usuario.Rol != RolTipo.Cliente;
    }
}
