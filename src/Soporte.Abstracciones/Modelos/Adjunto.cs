using System.ComponentModel.DataAnnotations;

namespace Soporte.Abstracciones.Modelos;

/// <summary>
/// Archivo adjunto a un ticket (HU-04) o a un comentario (HU-11): exactamente uno de
/// TicketId / ComentarioId tiene valor (CHECK en BD). No se guardan ambos para evitar
/// la dependencia transitiva ComentarioId → TicketId (3FN).
/// </summary>
public class Adjunto
{
    public const long TamanoMaximoBytes = 10 * 1024 * 1024;

    public int Id { get; set; }
    public int? TicketId { get; set; }
    public int? ComentarioId { get; set; }
    public int UsuarioId { get; set; }

    [Required, StringLength(255)]
    public string NombreOriginal { get; set; } = string.Empty;

    [Required, StringLength(100)]
    public string TipoContenido { get; set; } = string.Empty;

    [Range(1, TamanoMaximoBytes, ErrorMessage = "El archivo debe pesar menos de 10 MB.")]
    public long TamanoBytes { get; set; }

    [Required, StringLength(500)]
    public string RutaAlmacenamiento { get; set; } = string.Empty;

    public DateTime FechaSubida { get; set; }

    public Ticket? Ticket { get; set; }
    public Comentario? Comentario { get; set; }
    public Usuario? Usuario { get; set; }
}
