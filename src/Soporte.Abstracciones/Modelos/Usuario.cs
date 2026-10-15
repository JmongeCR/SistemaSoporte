using System.ComponentModel.DataAnnotations;

namespace Soporte.Abstracciones.Modelos;

public class Usuario
{
    public int Id { get; set; }

    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(100)]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "El correo es obligatorio.")]
    [EmailAddress(ErrorMessage = "El correo no tiene un formato válido.")]
    [StringLength(150)]
    public string Correo { get; set; } = string.Empty;

    [Required, StringLength(255)]
    public string PasswordHash { get; set; } = string.Empty;

    [Range(1, 3, ErrorMessage = "El rol debe ser Administrador, Agente o Cliente.")]
    public int RolId { get; set; }

    public bool Activo { get; set; } = true;

    public DateTime FechaCreacion { get; set; }

    /// <summary>Se incrementa cada vez que cambia la contraseña para invalidar tokens previos.</summary>
    public int ContrasenaVersion { get; set; }

    public Rol? Rol { get; set; }
}
