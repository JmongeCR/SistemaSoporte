using System.ComponentModel.DataAnnotations;
using Soporte.Abstracciones.Enumeraciones;

namespace Soporte.Abstracciones.Modelos;

/// <summary>Notificación dentro de la plataforma para un usuario sobre un ticket.</summary>
public class Notificacion
{
    public int Id { get; set; }

    /// <summary>Destinatario.</summary>
    public int UsuarioId { get; set; }
    public int TicketId { get; set; }
    public TipoNotificacion Tipo { get; set; }

    /// <summary>Texto tal como se generó al ocurrir el evento.</summary>
    [Required, StringLength(300)]
    public string Mensaje { get; set; } = string.Empty;

    /// <summary>Evento de origen (ej. "historial:15", "comentario:8"). Único por destinatario: evita duplicados.</summary>
    [Required, StringLength(100)]
    public string ClaveEvento { get; set; } = string.Empty;

    public DateTime FechaCreacion { get; set; }

    /// <summary>Null = no leída.</summary>
    public DateTime? FechaLectura { get; set; }

    public Usuario? Usuario { get; set; }
    public Ticket? Ticket { get; set; }
}
