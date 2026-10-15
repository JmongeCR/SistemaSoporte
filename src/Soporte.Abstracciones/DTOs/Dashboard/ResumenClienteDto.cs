namespace Soporte.Abstracciones.DTOs;

/// <summary>Dashboard del Cliente: conteo de sus propios casos.</summary>
public class ResumenClienteDto
{
    public int Abiertos { get; set; }
    public int EnProgreso { get; set; }

    /// <summary>Casos en los que el equipo pidió información y el cliente todavía no respondió.</summary>
    public int EsperanTuRespuesta { get; set; }
    public int Resueltos { get; set; }
    public int Cerrados { get; set; }
    public int Total { get; set; }
}
