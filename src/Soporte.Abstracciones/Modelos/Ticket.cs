using System.ComponentModel.DataAnnotations;

namespace Soporte.Abstracciones.Modelos;

public class Ticket
{
    public int Id { get; set; }

    [Required(ErrorMessage = "El asunto es obligatorio.")]
    [StringLength(150, MinimumLength = 5, ErrorMessage = "El asunto debe tener entre 5 y 150 caracteres.")]
    public string Asunto { get; set; } = string.Empty;

    [Required(ErrorMessage = "La descripción es obligatoria.")]
    [StringLength(4000, MinimumLength = 10, ErrorMessage = "La descripción debe tener entre 10 y 4000 caracteres.")]
    public string Descripcion { get; set; } = string.Empty;

    public int CategoriaId { get; set; }
    public int PrioridadId { get; set; }
    public int EstadoId { get; set; }
    public int ClienteId { get; set; }

    /// <summary>Null = ticket sin asignar (HU-04, HU-06).</summary>
    public int? AgenteAsignadoId { get; set; }

    public DateTime FechaCreacion { get; set; }
    public DateTime? FechaActualizacion { get; set; }

    /// <summary>Control de concurrencia optimista (HU-06: dos agentes toman el mismo ticket).</summary>
    [Timestamp]
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();

    public Categoria? Categoria { get; set; }
    public Prioridad? Prioridad { get; set; }
    public EstadoTicket? Estado { get; set; }
    public Usuario? Cliente { get; set; }
    public Usuario? AgenteAsignado { get; set; }

    public ICollection<Comentario> Comentarios { get; set; } = new List<Comentario>();
    public ICollection<Adjunto> Adjuntos { get; set; } = new List<Adjunto>();
    public ICollection<HistorialTicket> Historial { get; set; } = new List<HistorialTicket>();
}
