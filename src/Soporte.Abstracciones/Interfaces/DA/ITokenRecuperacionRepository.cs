using Soporte.Abstracciones.Modelos;

namespace Soporte.Abstracciones.Interfaces.DA;

public interface ITokenRecuperacionRepository
{
    Task CrearAsync(TokenRecuperacion token);
    Task<TokenRecuperacion?> ObtenerPorHashAsync(string tokenHash);
    Task MarcarUsadoAsync(int id);
    /// <summary>Actualiza Usado=true solo si el token aún estaba sin usar. Retorna true si fue el primero en marcarlo.</summary>
    Task<bool> MarcarUsadoSiDisponibleAsync(int id);
}
