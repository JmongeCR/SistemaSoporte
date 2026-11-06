namespace Soporte.Abstracciones.Modelos;

public class CalificacionCsat
{
    public int      Id                 { get; set; }
    public int      TicketId           { get; set; }
    public int      ClienteId          { get; set; }
    public int      Calificacion       { get; set; }
    public string?  Comentario         { get; set; }
    public DateTime FechaCalificacion  { get; set; }

    public Ticket  Ticket  { get; set; } = null!;
    public Usuario Cliente { get; set; } = null!;
}
