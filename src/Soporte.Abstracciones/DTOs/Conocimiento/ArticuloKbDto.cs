namespace Soporte.Abstracciones.DTOs;

public class ArticuloKbDto
{
    public int    Id               { get; set; }
    public string Titulo           { get; set; } = string.Empty;
    public string Contenido        { get; set; } = string.Empty;
    public string Slug             { get; set; } = string.Empty;
    public int    CategoriaId      { get; set; }
    public string CategoriaNombre  { get; set; } = string.Empty;
    public string CreadoPorNombre  { get; set; } = string.Empty;
    public DateTime FechaCreacion  { get; set; }
    public bool   Publicado        { get; set; }
    public int    Vistas           { get; set; }
}
