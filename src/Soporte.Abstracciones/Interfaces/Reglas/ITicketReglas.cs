using Soporte.Abstracciones.DTOs;
using Soporte.Abstracciones.Enumeraciones;
using Soporte.Abstracciones.Modelos;

namespace Soporte.Abstracciones.Interfaces.Reglas;

public interface ITicketReglas
{
    /// <summary>
    /// HU-04: todos los roles crean tickets. El Cliente, a su nombre; Administrador y Agente
    /// indican un cliente activo como solicitante.
    /// </summary>
    Task<ResultadoOperacion> ValidarCreacionAsync(CrearTicketDto ticket, UsuarioContextoDto usuario);

    /// <summary>HU-07/HU-08: transiciones válidas entre estados según el rol.</summary>
    ResultadoOperacion ValidarTransicionEstado(EstadoTicketTipo actual, EstadoTicketTipo nuevo, RolTipo rol);

    /// <summary>Cliente: solo sus tickets. Agente: asignados a él o sin asignar. Administrador: todos.</summary>
    bool PuedeVer(Ticket ticket, UsuarioContextoDto usuario);

    /// <summary>HU-07 esc. 2 y 3: el Cliente solo reabre; el Agente modifica sus tickets; solo el Administrador reasigna.</summary>
    ResultadoOperacion ValidarActualizacion(Ticket ticket, ActualizarTicketDto cambios, UsuarioContextoDto usuario);

    /// <summary>HU-07 esc. 3: el usuario destino de una reasignación debe ser un agente activo.</summary>
    Task<ResultadoOperacion> ValidarAgenteAsignableAsync(int agenteId);

    /// <summary>HU-06: solo Agentes toman tickets, y solo si están sin asignar.</summary>
    ResultadoOperacion ValidarTomaTicket(Ticket ticket, UsuarioContextoDto usuario);
}
