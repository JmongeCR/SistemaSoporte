using Soporte.Abstracciones.DTOs;
using Soporte.Abstracciones.Modelos;

namespace Soporte.Abstracciones.Interfaces.DA;

public interface ITicketRepository
{
    /// <param name="filtro">Filtros elegidos por el usuario (HU-05).</param>
    /// <param name="alcanceAgenteId">Si tiene valor, limita a tickets de ese agente o sin asignar (HU-05 esc. 1).</param>
    /// <param name="alcanceClienteId">Si tiene valor, limita a los tickets de ese cliente ("Mis tickets").</param>
    Task<ResultadoPaginado<TicketResumenDto>> ListarAsync(FiltroTicketsDto filtro, int? alcanceAgenteId, int? alcanceClienteId);
    Task<Ticket?> ObtenerPorIdAsync(int id);
    Task<TicketDetalleDto?> ObtenerDetalleAsync(int id, bool incluirNotasInternas, int lectorId);

    /// <summary>Inserta el ticket junto con los registros de <see cref="Ticket.Historial"/> agregados.</summary>
    Task<int> CrearAsync(Ticket ticket);

    /// <summary>Guarda los cambios del ticket (y su historial nuevo) si nadie lo modificó desde que se leyó.</summary>
    /// <returns>false si el RowVersion ya no coincide (otro usuario lo modificó).</returns>
    Task<bool> ActualizarAsync(Ticket ticket, byte[] rowVersionEsperado);

    /// <summary>Asignación atómica vía sp_TomarTicket (HU-06).</summary>
    /// <returns>Asignado = false si otro agente ya lo tomó; HistorialId del cambio de estado, si lo hubo.</returns>
    Task<(bool Asignado, int? HistorialId)> TomarTicketAsync(int ticketId, int agenteId);

    /// <summary>Actualiza la fecha de última actividad visible (ej. una respuesta pública).</summary>
    Task RegistrarActividadAsync(int ticketId, DateTime fechaUtc);
}
