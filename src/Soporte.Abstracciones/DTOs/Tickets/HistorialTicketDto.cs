namespace Soporte.Abstracciones.DTOs;

public class HistorialTicketDto
{
    public string Usuario { get; init; } = string.Empty;
    public string? EstadoAnterior { get; init; }
    public int EstadoNuevoId { get; init; }
    public string EstadoNuevo { get; init; } = string.Empty;
    public DateTime FechaCambio { get; init; }
}
