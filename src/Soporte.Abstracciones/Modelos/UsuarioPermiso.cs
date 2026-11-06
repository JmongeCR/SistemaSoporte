namespace Soporte.Abstracciones.Modelos;

public class UsuarioPermiso
{
    public int UsuarioId { get; set; }
    public int PermisoId { get; set; }
    public bool Concedido { get; set; }

    public Usuario? Usuario { get; set; }
    public Permiso? Permiso { get; set; }
}
