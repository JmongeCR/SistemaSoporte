using System.ComponentModel.DataAnnotations;

namespace Soporte.Abstracciones.Modelos;

public class Rol
{
    public int Id { get; set; }

    [Required, StringLength(30)]
    public string Nombre { get; set; } = string.Empty;

    public ICollection<Usuario> Usuarios { get; set; } = new List<Usuario>();
    public ICollection<RolPermiso> Permisos { get; set; } = new List<RolPermiso>();
}
