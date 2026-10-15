using Soporte.Abstracciones.DTOs;

namespace Soporte.API.Servicios;

/// <summary>Obtiene el usuario que hace la petición, para pasarlo a la capa de Flujo.</summary>
public interface IUsuarioActualProveedor
{
    /// <returns>null si la petición no está autenticada.</returns>
    UsuarioContextoDto? Obtener();
}
