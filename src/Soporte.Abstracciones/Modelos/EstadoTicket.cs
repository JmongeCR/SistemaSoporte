using System.ComponentModel.DataAnnotations;

namespace Soporte.Abstracciones.Modelos;

/// <summary>Catálogo: Abierto, En Progreso, Resuelto, Cerrado (ver <see cref="Enumeraciones.EstadoTicketTipo"/>).</summary>
public class EstadoTicket
{
    public int Id { get; set; }

    [Required, StringLength(30)]
    public string Nombre { get; set; } = string.Empty;
}
