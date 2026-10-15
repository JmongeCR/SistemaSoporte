namespace Soporte.Abstracciones.DTOs;

public class UsuarioDto
{
    public int Id { get; init; }
    public string Nombre { get; init; } = string.Empty;
    public string Correo { get; init; } = string.Empty;
    public int RolId { get; init; }
    public string Rol { get; init; } = string.Empty;
    public bool Activo { get; init; }
}
