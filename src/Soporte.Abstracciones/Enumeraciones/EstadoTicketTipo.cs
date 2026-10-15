namespace Soporte.Abstracciones.Enumeraciones;

/// <summary>Estados del ticket. Los valores coinciden con los Id sembrados en la tabla EstadoTicket.</summary>
public enum EstadoTicketTipo
{
    Abierto = 1,
    EnProgreso = 2,
    Resuelto = 3,
    Cerrado = 4
}
