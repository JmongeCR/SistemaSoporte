using Soporte.Abstracciones.DTOs;

namespace Soporte.Abstracciones.Interfaces.Flujo;

public interface IAutenticacionFlujo
{
    Task<ResultadoOperacion<SesionDto>> IniciarSesionAsync(LoginSolicitudDto dto);

    /// <summary>HU-02 esc. 2: siempre responde con un mensaje genérico, exista o no el correo.</summary>
    Task<ResultadoOperacion> SolicitarRecuperacionAsync(SolicitudRecuperacionDto dto);
    Task<ResultadoOperacion> RestablecerContrasenaAsync(RestablecerContrasenaDto dto);
}
