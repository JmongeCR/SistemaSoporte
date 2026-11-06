namespace Soporte.Abstracciones.DTOs;

public class ActualizarPermisosUsuarioDto
{
    /// <summary>IDs de los permisos que el usuario tendrá habilitados (los ausentes quedan deshabilitados).</summary>
    public IReadOnlyList<int> PermisosHabilitados { get; init; } = Array.Empty<int>();
}
