namespace Soporte.Abstracciones.DTOs;

/// <summary>HU-16: permisos del rol agrupados, de solo lectura.</summary>
public class MatrizPermisosDto
{
    public int RolId { get; init; }
    public string Rol { get; init; } = string.Empty;
    public IReadOnlyList<GrupoPermisosDto> Grupos { get; init; } = Array.Empty<GrupoPermisosDto>();
}

public class GrupoPermisosDto
{
    public string Grupo { get; init; } = string.Empty;
    public IReadOnlyList<PermisoDto> Permisos { get; init; } = Array.Empty<PermisoDto>();
}

public class PermisoDto
{
    public string Codigo { get; init; } = string.Empty;
    public string Descripcion { get; init; } = string.Empty;
    public bool Concedido { get; init; }
}
