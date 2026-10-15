using Soporte.Abstracciones.Enumeraciones;

namespace Soporte.Abstracciones.DTOs;

/// <summary>Usuario autenticado que ejecuta la operación. Lo construye la API a partir de los claims.</summary>
public class UsuarioContextoDto
{
    public int UsuarioId { get; init; }
    public RolTipo Rol { get; init; }
}
