namespace Soporte.Abstracciones.DTOs;

public class ResultadoPaginado<T>
{
    public IReadOnlyList<T> Elementos { get; init; } = Array.Empty<T>();
    public int Total { get; init; }
    public int Pagina { get; init; }
    public int TamanoPagina { get; init; }
    public int TotalPaginas => TamanoPagina == 0 ? 0 : (int)Math.Ceiling(Total / (double)TamanoPagina);
}
