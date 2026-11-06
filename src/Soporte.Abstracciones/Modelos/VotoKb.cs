namespace Soporte.Abstracciones.Modelos;

public class VotoKb
{
    public int      ArticuloKbId { get; set; }
    public int      UsuarioId    { get; set; }
    public bool     EsUtil       { get; set; }
    public DateTime FechaVoto    { get; set; }

    public ArticuloKb Articulo { get; set; } = null!;
    public Usuario    Usuario  { get; set; } = null!;
}
