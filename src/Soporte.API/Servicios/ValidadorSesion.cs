using System.Security.Claims;
using Soporte.Abstracciones.Enumeraciones;
using Soporte.Abstracciones.Interfaces.DA;

namespace Soporte.API.Servicios;

/// <summary>
/// Un cambio de rol (HU-15) o una cuenta desactivada invalida los tokens ya emitidos:
/// sin esta verificación, un usuario conservaría el rol anterior hasta que venza su token.
/// </summary>
public class ValidadorSesion : IValidadorSesion
{
    private readonly IUsuarioRepository _usuarios;

    public ValidadorSesion(IUsuarioRepository usuarios)
    {
        _usuarios = usuarios;
    }

    public async Task<bool> EsVigenteAsync(ClaimsPrincipal usuario)
    {
        if (!int.TryParse(usuario.FindFirstValue(ClaimTypes.NameIdentifier), out var id)
            || !Enum.TryParse<RolTipo>(usuario.FindFirstValue(ClaimTypes.Role), out var rol))
            return false;

        if (!int.TryParse(usuario.FindFirstValue("cv"), out var versionToken))
            return false;

        var actual = await _usuarios.ObtenerPorIdAsync(id);
        return actual is { Activo: true }
            && actual.RolId == (int)rol
            && actual.ContrasenaVersion == versionToken;
    }
}
