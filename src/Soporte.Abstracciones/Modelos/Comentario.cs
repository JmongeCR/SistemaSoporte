using System.ComponentModel.DataAnnotations;

namespace Soporte.Abstracciones.Modelos;

public class Comentario
{
    public int Id { get; set; }
    public int TicketId { get; set; }
    public int UsuarioId { get; set; }

    [Required(ErrorMessage = "El comentario no puede estar vacío.")]
    [StringLength(4000)]
    public string Texto { get; set; } = string.Empty;

    /// <summary>true = solo visible para Agentes y Administradores (HU-10).</summary>
    public bool EsNotaInterna { get; set; }

    /// <summary>El equipo pide información adicional al cliente. Siempre es un comentario público.</summary>
    public bool SolicitaInformacion { get; set; }

    public DateTime FechaCreacion { get; set; }

    public Ticket? Ticket { get; set; }
    public Usuario? Usuario { get; set; }

    public ICollection<Adjunto> Adjuntos { get; set; } = new List<Adjunto>();
    public ICollection<LecturaComentario> Lecturas { get; set; } = new List<LecturaComentario>();
}
