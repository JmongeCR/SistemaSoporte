using Soporte.Abstracciones.DTOs;

namespace Soporte.Web.Servicios;

public interface IAutenticacionApiServicio
{
    Task<ResultadoOperacion<SesionDto>> IniciarSesionAsync(LoginSolicitudDto credenciales);
    Task<ResultadoOperacion<MensajeDto>> SolicitarRecuperacionAsync(SolicitudRecuperacionDto solicitud);
    Task<ResultadoOperacion<MensajeDto>> RestablecerContrasenaAsync(RestablecerContrasenaDto datos);
}
