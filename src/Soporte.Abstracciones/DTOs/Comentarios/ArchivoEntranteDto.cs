namespace Soporte.Abstracciones.DTOs;

/// <summary>Archivo recibido por la API, desacoplado de IFormFile para que Flujo y Reglas no dependan de ASP.NET.</summary>
public class ArchivoEntranteDto
{
    public string NombreOriginal { get; init; } = string.Empty;
    public string TipoContenido { get; init; } = string.Empty;
    public long TamanoBytes { get; init; }
    public required Stream Contenido { get; init; }
}
