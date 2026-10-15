namespace Soporte.Abstracciones.Modelos;

/// <summary>Bitácora de cambios de estado de un ticket (HU-07, HU-08).</summary>
public class HistorialTicket
{
    public int Id { get; set; }
    public int TicketId { get; set; }
    public int UsuarioId { get; set; }

    /// <summary>Null en el registro de creación del ticket.</summary>
    public int? EstadoAnteriorId { get; set; }
    public int EstadoNuevoId { get; set; }

    public DateTime FechaCambio { get; set; }

    public Ticket? Ticket { get; set; }
    public Usuario? Usuario { get; set; }
    public EstadoTicket? EstadoAnterior { get; set; }
    public EstadoTicket? EstadoNuevo { get; set; }
}
