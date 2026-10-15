using System.ComponentModel.DataAnnotations;

namespace Soporte.Abstracciones.DTOs;

/// <summary>HU-14: búsqueda por nombre/correo y filtro por rol.</summary>
public class FiltroUsuariosDto
{
    [StringLength(100)]
    public string? Texto { get; set; }

    [Range(1, 3)]
    public int? RolId { get; set; }
}
