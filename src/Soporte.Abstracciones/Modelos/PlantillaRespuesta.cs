namespace Soporte.Abstracciones.Modelos;

public class PlantillaRespuesta
{
    public int      Id            { get; set; }
    public string   Titulo        { get; set; } = string.Empty;
    public string   Texto         { get; set; } = string.Empty;
    public int      CreadoPorId   { get; set; }
    public DateTime FechaCreacion { get; set; }
    public bool     Activa        { get; set; } = true;

    public Usuario CreadoPor { get; set; } = null!;
}
