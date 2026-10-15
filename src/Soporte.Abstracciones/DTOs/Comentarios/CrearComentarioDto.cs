using System.ComponentModel.DataAnnotations;

namespace Soporte.Abstracciones.DTOs;

public class CrearComentarioDto
{
    [Required(ErrorMessage = "El comentario no puede estar vacío.")]
    [StringLength(4000, ErrorMessage = "El comentario no puede superar los 4000 caracteres.")]
    public string Texto { get; set; } = string.Empty;

    /// <summary>HU-10. Si el autor es Cliente, la regla de negocio lo rechaza.</summary>
    public bool EsNotaInterna { get; set; }

    /// <summary>El equipo pide información adicional al cliente (genera una notificación específica).</summary>
    public bool SolicitaInformacion { get; set; }
}
