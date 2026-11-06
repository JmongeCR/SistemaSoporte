namespace Soporte.Abstracciones.DTOs;

public class CalificacionCsatDto
{
    public int      TicketId          { get; init; }
    public int      Calificacion      { get; init; }
    public string?  Comentario        { get; init; }
    public DateTime FechaCalificacion { get; init; }
}
