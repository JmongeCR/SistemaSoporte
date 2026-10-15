using Soporte.Abstracciones.DTOs;

namespace Soporte.Web.Servicios;

public class AutenticacionApiServicio : IAutenticacionApiServicio
{
    private readonly IApiSoporteCliente _api;

    public AutenticacionApiServicio(IApiSoporteCliente api)
    {
        _api = api;
    }

    public async Task<ResultadoOperacion<SesionDto>> IniciarSesionAsync(LoginSolicitudDto credenciales) =>
        await _api.PostJsonAsync<SesionDto>("api/autenticacion/login", credenciales);

    public async Task<ResultadoOperacion<MensajeDto>> SolicitarRecuperacionAsync(SolicitudRecuperacionDto solicitud) =>
        await _api.PostJsonAsync<MensajeDto>("api/autenticacion/recuperacion", solicitud);

    public async Task<ResultadoOperacion<MensajeDto>> RestablecerContrasenaAsync(RestablecerContrasenaDto datos) =>
        await _api.PostJsonAsync<MensajeDto>("api/autenticacion/restablecimiento", datos);
}
