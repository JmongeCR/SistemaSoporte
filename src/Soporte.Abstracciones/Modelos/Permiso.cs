using System.ComponentModel.DataAnnotations;

namespace Soporte.Abstracciones.Modelos;

/// <summary>Permiso de solo lectura para la matriz de permisos (HU-16).</summary>
public class Permiso
{
    public int Id { get; set; }

    [Required, StringLength(50)]
    public string Codigo { get; set; } = string.Empty;

    [Required, StringLength(200)]
    public string Descripcion { get; set; } = string.Empty;

    /// <summary>Visibilidad, Gestión de tickets, Comunicación interna o Administración.</summary>
    [Required, StringLength(50)]
    public string Grupo { get; set; } = string.Empty;

    public ICollection<RolPermiso> Roles { get; set; } = new List<RolPermiso>();
}
