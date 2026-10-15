namespace Soporte.Abstracciones.DTOs;

/// <summary>Resultado de sp_ObtenerMetricasDashboard. Los nombres coinciden con las columnas del SP.</summary>
public class MetricasDashboardDto
{
    public int Abiertos { get; set; }
    public int EnProgreso { get; set; }
    public int ResueltosHoy { get; set; }
    public int SlaVencido { get; set; }
}
