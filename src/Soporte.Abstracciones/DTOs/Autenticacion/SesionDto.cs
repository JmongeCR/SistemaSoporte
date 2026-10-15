namespace Soporte.Abstracciones.DTOs;

public class SesionDto
{
    public int UsuarioId { get; init; }
    public string Nombre { get; init; } = string.Empty;
    public string Correo { get; init; } = string.Empty;
    public int RolId { get; init; }
    public string Rol { get; init; } = string.Empty;
    public string Token { get; init; } = string.Empty;
    /// <summary>Vencimiento del token, en UTC.</summary>
    public DateTime Expira { get; init; }
}
