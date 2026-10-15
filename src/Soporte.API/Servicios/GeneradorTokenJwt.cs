using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Soporte.Abstracciones.Interfaces.Infraestructura;
using Soporte.Abstracciones.Modelos;

namespace Soporte.API.Servicios;

public class GeneradorTokenJwt : IGeneradorToken
{
    private readonly ConfiguracionJwt _configuracion;
    private readonly JsonWebTokenHandler _manejador = new();

    public GeneradorTokenJwt(ConfiguracionJwt configuracion)
    {
        _configuracion = configuracion;
    }

    public (string Token, DateTime ExpiraUtc) Generar(Usuario usuario)
    {
        var ahora = DateTime.UtcNow;
        var expira = ahora.AddMinutes(_configuracion.MinutosExpiracion);

        var descriptor = new SecurityTokenDescriptor
        {
            Issuer = _configuracion.Emisor,
            Audience = _configuracion.Audiencia,
            IssuedAt = ahora,
            NotBefore = ahora,
            Expires = expira,
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                new Claim(ClaimTypes.Name, usuario.Nombre),
                new Claim(ClaimTypes.Email, usuario.Correo),
                new Claim(ClaimTypes.Role, usuario.Rol!.Nombre),
                new Claim("cv", usuario.ContrasenaVersion.ToString())
            }),
            SigningCredentials = new SigningCredentials(CrearClave(_configuracion), SecurityAlgorithms.HmacSha256)
        };

        return (_manejador.CreateToken(descriptor), expira);
    }

    public static SymmetricSecurityKey CrearClave(ConfiguracionJwt configuracion) =>
        new(Encoding.UTF8.GetBytes(configuracion.Clave));
}
