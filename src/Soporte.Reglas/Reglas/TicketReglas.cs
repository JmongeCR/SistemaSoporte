using Soporte.Abstracciones.DTOs;
using Soporte.Abstracciones.Enumeraciones;
using Soporte.Abstracciones.Interfaces.DA;
using Soporte.Abstracciones.Interfaces.Reglas;
using Soporte.Abstracciones.Modelos;

namespace Soporte.Reglas.Reglas;

public class TicketReglas : ITicketReglas
{
    /// <summary>
    /// Transiciones permitidas a Agentes y Administradores (HU-07, HU-08).
    /// Reabrir un ticket Cerrado queda reservado al Administrador; el Cliente solo reabre uno Resuelto.
    /// </summary>
    private static readonly HashSet<(EstadoTicketTipo Desde, EstadoTicketTipo Hacia)> TransicionesEquipo = new()
    {
        (EstadoTicketTipo.Abierto, EstadoTicketTipo.EnProgreso),
        (EstadoTicketTipo.EnProgreso, EstadoTicketTipo.Abierto),
        (EstadoTicketTipo.EnProgreso, EstadoTicketTipo.Resuelto),
        (EstadoTicketTipo.Resuelto, EstadoTicketTipo.Abierto),
        (EstadoTicketTipo.Resuelto, EstadoTicketTipo.Cerrado)
    };

    private readonly IUsuarioRepository _usuarios;

    public TicketReglas(IUsuarioRepository usuarios)
    {
        _usuarios = usuarios;
    }

    public async Task<ResultadoOperacion> ValidarCreacionAsync(CrearTicketDto ticket, UsuarioContextoDto usuario)
    {
        if (usuario.Rol == RolTipo.Cliente)
        {
            return ticket.ClienteId is null || ticket.ClienteId == usuario.UsuarioId
                ? ResultadoOperacion.Ok()
                : ResultadoOperacion.SinPermiso("Un cliente solo puede crear tickets a su nombre.");
        }

        if (ticket.ClienteId is not int clienteId)
            return ResultadoOperacion.Fallo("Seleccione el cliente solicitante.");

        var cliente = await _usuarios.ObtenerPorIdAsync(clienteId);
        return cliente is { RolId: (int)RolTipo.Cliente, Activo: true }
            ? ResultadoOperacion.Ok()
            : ResultadoOperacion.Fallo("El solicitante seleccionado no es un cliente activo.");
    }

    public ResultadoOperacion ValidarTransicionEstado(EstadoTicketTipo actual, EstadoTicketTipo nuevo, RolTipo rol)
    {
        if (actual == nuevo)
            return ResultadoOperacion.Ok();

        if (rol == RolTipo.Cliente)
        {
            return actual == EstadoTicketTipo.Resuelto && nuevo == EstadoTicketTipo.Abierto
                ? ResultadoOperacion.Ok()
                : ResultadoOperacion.SinPermiso("El cliente solo puede reabrir un ticket resuelto.");
        }

        if (actual == EstadoTicketTipo.Cerrado && nuevo == EstadoTicketTipo.Abierto)
        {
            return rol == RolTipo.Administrador
                ? ResultadoOperacion.Ok()
                : ResultadoOperacion.SinPermiso("Solo un administrador puede reabrir un ticket cerrado.");
        }

        return TransicionesEquipo.Contains((actual, nuevo))
            ? ResultadoOperacion.Ok()
            : ResultadoOperacion.Fallo($"No se permite cambiar el estado de {NombreEstado(actual)} a {NombreEstado(nuevo)}.");
    }

    public bool PuedeVer(Ticket ticket, UsuarioContextoDto usuario)
    {
        return usuario.Rol switch
        {
            RolTipo.Administrador => true,
            RolTipo.Agente => ticket.AgenteAsignadoId is null || ticket.AgenteAsignadoId == usuario.UsuarioId,
            RolTipo.Cliente => ticket.ClienteId == usuario.UsuarioId,
            _ => false
        };
    }

    public ResultadoOperacion ValidarActualizacion(Ticket ticket, ActualizarTicketDto cambios, UsuarioContextoDto usuario)
    {
        var actual = (EstadoTicketTipo)ticket.EstadoId;
        var nuevo = (EstadoTicketTipo)cambios.EstadoId;
        var cambiaDatos = cambios.PrioridadId != ticket.PrioridadId || cambios.CategoriaId != ticket.CategoriaId;
        var cambiaAgente = cambios.AgenteAsignadoId != ticket.AgenteAsignadoId;

        switch (usuario.Rol)
        {
            case RolTipo.Cliente when cambiaDatos || cambiaAgente:
                return ResultadoOperacion.SinPermiso("El cliente no puede modificar la prioridad, la categoría ni el agente del ticket.");
            case RolTipo.Agente when ticket.AgenteAsignadoId != usuario.UsuarioId:
                return ResultadoOperacion.SinPermiso("Solo puede modificar tickets asignados a usted. Tome el ticket primero.");
            case RolTipo.Agente when cambiaAgente:
                return ResultadoOperacion.SinPermiso("Solo un administrador puede reasignar tickets.");
        }

        var conMensaje = !string.IsNullOrWhiteSpace(cambios.MensajePublico);

        if (actual == EstadoTicketTipo.Cerrado && nuevo == EstadoTicketTipo.Cerrado && (cambiaDatos || cambiaAgente || conMensaje))
            return ResultadoOperacion.Fallo("Un ticket cerrado no se puede modificar; primero debe reabrirse.");

        // El cliente responde por la conversación; aquí solo puede acompañar su reapertura con un motivo.
        if (usuario.Rol == RolTipo.Cliente && conMensaje && actual == nuevo)
            return ResultadoOperacion.Fallo("Para responder, use la conversación del ticket.");

        var transicion = ValidarTransicionEstado(actual, nuevo, usuario.Rol);
        if (!transicion.Exito)
            return transicion;

        if ((nuevo == EstadoTicketTipo.EnProgreso || nuevo == EstadoTicketTipo.Resuelto) && cambios.AgenteAsignadoId is null)
            return ResultadoOperacion.Fallo($"Un ticket {NombreEstado(nuevo)} debe tener un agente asignado.");

        return ResultadoOperacion.Ok();
    }

    public async Task<ResultadoOperacion> ValidarAgenteAsignableAsync(int agenteId)
    {
        var agente = await _usuarios.ObtenerPorIdAsync(agenteId);

        return agente is { RolId: (int)RolTipo.Agente, Activo: true }
            ? ResultadoOperacion.Ok()
            : ResultadoOperacion.Fallo("El usuario seleccionado no es un agente activo.");
    }

    public ResultadoOperacion ValidarTomaTicket(Ticket ticket, UsuarioContextoDto usuario)
    {
        if (usuario.Rol != RolTipo.Agente)
            return ResultadoOperacion.SinPermiso("Solo los agentes pueden tomar tickets.");

        if (ticket.AgenteAsignadoId is not null)
            return ResultadoOperacion.Conflicto("El ticket ya fue asignado a otro agente.");

        var estado = (EstadoTicketTipo)ticket.EstadoId;
        if (estado != EstadoTicketTipo.Abierto && estado != EstadoTicketTipo.EnProgreso)
            return ResultadoOperacion.Fallo("Solo se pueden tomar tickets abiertos o en progreso.");

        return ResultadoOperacion.Ok();
    }

    private static string NombreEstado(EstadoTicketTipo estado) => estado switch
    {
        EstadoTicketTipo.EnProgreso => "En Progreso",
        _ => estado.ToString()
    };
}
