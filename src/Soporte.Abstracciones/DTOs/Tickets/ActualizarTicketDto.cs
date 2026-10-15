using System.ComponentModel.DataAnnotations;

namespace Soporte.Abstracciones.DTOs;

/// <summary>HU-07 / HU-08: cambio de estado, prioridad, categoría y reasignación (solo Administrador).</summary>
public class ActualizarTicketDto
{
    [Range(1, 4, ErrorMessage = "Seleccione un estado válido.")]
    public int EstadoId { get; set; }

    [Range(1, 4, ErrorMessage = "Seleccione una prioridad válida.")]
    public int PrioridadId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Seleccione una categoría.")]
    public int CategoriaId { get; set; }

    public int? AgenteAsignadoId { get; set; }

    /// <summary>
    /// Mensaje público opcional que acompaña el cambio (ej. explicación de la resolución o motivo de la reapertura).
    /// Se guarda como comentario y se notifica una sola vez junto con el cambio de estado.
    /// </summary>
    [StringLength(4000, ErrorMessage = "El mensaje no puede superar los 4000 caracteres.")]
    public string? MensajePublico { get; set; }

    /// <summary>RowVersion en Base64 recibido al leer el ticket; detecta ediciones concurrentes.</summary>
    [Required]
    public string RowVersion { get; set; } = string.Empty;
}
