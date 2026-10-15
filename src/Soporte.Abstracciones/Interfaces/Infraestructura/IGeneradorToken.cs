using Soporte.Abstracciones.Modelos;

namespace Soporte.Abstracciones.Interfaces.Infraestructura;

/// <summary>Emite el token de acceso (JWT) con el que Soporte.Web llama al API en nombre del usuario.</summary>
public interface IGeneradorToken
{
    /// <param name="usuario">Usuario autenticado, con su <see cref="Usuario.Rol"/> cargado.</param>
    /// <returns>El token y su vencimiento en UTC.</returns>
    (string Token, DateTime ExpiraUtc) Generar(Usuario usuario);
}
