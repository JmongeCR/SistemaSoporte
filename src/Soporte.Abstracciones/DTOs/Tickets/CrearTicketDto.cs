using System.ComponentModel.DataAnnotations;

namespace Soporte.Abstracciones.DTOs;

/// <summary>HU-04. Lo crean todos los roles (según el diseño "05 - Crear Ticket").</summary>
public class CrearTicketDto
{
    [Required(ErrorMessage = "El asunto es obligatorio.")]
    [StringLength(150, MinimumLength = 5, ErrorMessage = "El asunto debe tener entre 5 y 150 caracteres.")]
    public string Asunto { get; set; } = string.Empty;

    [Required(ErrorMessage = "La descripción es obligatoria.")]
    [StringLength(4000, MinimumLength = 10, ErrorMessage = "La descripción debe tener entre 10 y 4000 caracteres.")]
    public string Descripcion { get; set; } = string.Empty;

    [Range(1, int.MaxValue, ErrorMessage = "Seleccione una categoría.")]
    public int CategoriaId { get; set; }

    [Range(1, 4, ErrorMessage = "Seleccione una prioridad válida.")]
    public int PrioridadId { get; set; }

    /// <summary>
    /// Cliente solicitante. Obligatorio para Administrador y Agente; un Cliente lo omite
    /// (se usa su propio Id) y no puede indicar a otro cliente.
    /// </summary>
    public int? ClienteId { get; set; }
}
