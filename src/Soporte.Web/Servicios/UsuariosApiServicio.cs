using Soporte.Abstracciones.DTOs;

namespace Soporte.Web.Servicios;

public class UsuariosApiServicio : IUsuariosApiServicio
{
    private readonly IApiSoporteCliente _api;

    public UsuariosApiServicio(IApiSoporteCliente api)
    {
        _api = api;
    }

    public async Task<ResultadoOperacion<IReadOnlyList<UsuarioDto>>> ListarAsync(FiltroUsuariosDto filtro)
    {
        var qs = new List<string>();
        if (!string.IsNullOrWhiteSpace(filtro.Texto)) qs.Add($"Texto={Uri.EscapeDataString(filtro.Texto)}");
        if (filtro.RolId.HasValue) qs.Add($"RolId={filtro.RolId}");
        var url = qs.Count > 0 ? $"api/usuarios?{string.Join("&", qs)}" : "api/usuarios";
        return await _api.GetAsync<IReadOnlyList<UsuarioDto>>(url);
    }

    public async Task<ResultadoOperacion<IReadOnlyList<UsuarioDto>>> ListarClientesAsync() =>
        await _api.GetAsync<IReadOnlyList<UsuarioDto>>("api/usuarios/clientes");

    public async Task<ResultadoOperacion<IReadOnlyList<UsuarioDto>>> ListarAgentesAsync() =>
        await _api.GetAsync<IReadOnlyList<UsuarioDto>>("api/usuarios?RolId=2");

    public async Task<ResultadoOperacion<UsuarioDto>> ActualizarPerfilAsync(ActualizarPerfilDto dto) =>
        await _api.PutJsonAsync<UsuarioDto>("api/cuenta/perfil", dto);

    public async Task<ResultadoOperacion<UsuarioDto>> CambiarRolAsync(int id, CambiarRolDto dto) =>
        await _api.PutJsonAsync<UsuarioDto>($"api/usuarios/{id}/rol", dto);

    public async Task<ResultadoOperacion<MatrizPermisosDto>> ObtenerPermisosAsync(int id) =>
        await _api.GetAsync<MatrizPermisosDto>($"api/usuarios/{id}/permisos");

    public async Task<ResultadoOperacion> ActualizarPermisosAsync(int id, ActualizarPermisosUsuarioDto dto) =>
        await _api.PutJsonAsync($"api/usuarios/{id}/permisos", dto);

    public async Task<ResultadoOperacion> RestablecerPermisosAsync(int id) =>
        await _api.DeleteAsync($"api/usuarios/{id}/permisos");
}
