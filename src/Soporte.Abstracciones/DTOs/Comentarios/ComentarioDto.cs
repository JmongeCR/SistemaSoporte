namespace Soporte.Abstracciones.DTOs;

public class ComentarioDto
{
    public int Id { get; init; }
    public int TicketId { get; init; }
    public int UsuarioId { get; init; }
    public string Autor { get; init; } = string.Empty;
    public string RolAutor { get; init; } = string.Empty;
    public string Texto { get; init; } = string.Empty;
    public bool EsNotaInterna { get; init; }
    public bool SolicitaInformacion { get; init; }
    public DateTime FechaCreacion { get; init; }

    /// <summary>HU-09: si el usuario que consulta ya leyó el comentario.</summary>
    public bool Leido { get; init; }

    public IReadOnlyList<AdjuntoDto> Adjuntos { get; init; } = Array.Empty<AdjuntoDto>();
}
