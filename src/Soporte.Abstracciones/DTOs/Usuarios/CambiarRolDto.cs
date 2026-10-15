using System.ComponentModel.DataAnnotations;

namespace Soporte.Abstracciones.DTOs;

/// <summary>HU-15.</summary>
public class CambiarRolDto
{
    [Range(1, 3, ErrorMessage = "El rol debe ser Administrador, Agente o Cliente.")]
    public int RolId { get; set; }
}
