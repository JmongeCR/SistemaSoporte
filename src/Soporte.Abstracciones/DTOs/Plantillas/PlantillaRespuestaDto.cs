namespace Soporte.Abstracciones.DTOs;

public class PlantillaRespuestaDto
{
    public int    Id     { get; init; }
    public string Titulo { get; init; } = string.Empty;
    public string Texto  { get; init; } = string.Empty;
    public bool   Activa { get; init; }
}
