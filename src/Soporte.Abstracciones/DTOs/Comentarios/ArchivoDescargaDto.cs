namespace Soporte.Abstracciones.DTOs;

/// <summary>Archivo listo para enviarse al navegador. Quien lo recibe es responsable de cerrar el Stream.</summary>
public class ArchivoDescargaDto
{
    public string NombreOriginal { get; init; } = string.Empty;
    public string TipoContenido { get; init; } = string.Empty;
    public required Stream Contenido { get; init; }
}
