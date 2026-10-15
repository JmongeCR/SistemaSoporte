namespace Soporte.Abstracciones.DTOs;

public class AdjuntoDto
{
    public int Id { get; init; }
    public string NombreOriginal { get; init; } = string.Empty;
    public string TipoContenido { get; init; } = string.Empty;
    public long TamanoBytes { get; init; }
    public DateTime FechaSubida { get; init; }
}
