namespace Soporte.Abstracciones.Modelos;

public class ArticuloKb
{
    public int      Id                 { get; set; }
    public string   Titulo             { get; set; } = string.Empty;
    public string   Contenido          { get; set; } = string.Empty;
    public string   Slug               { get; set; } = string.Empty;
    public int      CategoriaId        { get; set; }
    public int      CreadoPorId        { get; set; }
    public DateTime FechaCreacion      { get; set; }
    public DateTime? FechaActualizacion { get; set; }
    public bool     Publicado          { get; set; } = true;
    public int      Vistas             { get; set; }

    public Categoria Categoria  { get; set; } = null!;
    public Usuario   CreadoPor  { get; set; } = null!;
    public ICollection<TicketArticuloKb> Tickets { get; set; } = new List<TicketArticuloKb>();
}
