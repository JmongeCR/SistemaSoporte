namespace Soporte.Abstracciones.Modelos;

public class TicketArticuloKb
{
    public int      TicketId       { get; set; }
    public int      ArticuloKbId   { get; set; }
    public int      VinculadoPorId { get; set; }
    public DateTime FechaVinculo   { get; set; }

    public Ticket     Ticket     { get; set; } = null!;
    public ArticuloKb Articulo   { get; set; } = null!;
    public Usuario    VinculadoPor { get; set; } = null!;
}
