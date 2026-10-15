using System.Security.Claims;

namespace Soporte.API.Servicios;

/// <summary>Confirma que el usuario de un token sigue activo y con el mismo rol que tenía al emitirse.</summary>
public interface IValidadorSesion
{
    Task<bool> EsVigenteAsync(ClaimsPrincipal usuario);
}
