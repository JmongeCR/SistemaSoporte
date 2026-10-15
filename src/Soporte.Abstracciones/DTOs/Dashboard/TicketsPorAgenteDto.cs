namespace Soporte.Abstracciones.DTOs;

/// <summary>Resultado de sp_TicketsPorAgente. Los nombres coinciden con las columnas del SP.</summary>
public class TicketsPorAgenteDto
{
    public int AgenteId { get; set; }
    public string Agente { get; set; } = string.Empty;
    public int Abiertos { get; set; }
    public int EnProgreso { get; set; }
    public int Resueltos { get; set; }
    public int Cerrados { get; set; }
}
