namespace Soporte.Abstracciones.DTOs;

public class ArticuloKbResumenDto
{
    public int    Id              { get; set; }
    public string Titulo          { get; set; } = string.Empty;
    public string Slug            { get; set; } = string.Empty;
    public string CategoriaNombre { get; set; } = string.Empty;
    public int    Vistas          { get; set; }
}
