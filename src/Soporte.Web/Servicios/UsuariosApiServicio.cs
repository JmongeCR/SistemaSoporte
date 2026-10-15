using Microsoft.AspNetCore.WebUtilities;
using Soporte.Abstracciones.DTOs;

namespace Soporte.Web.Servicios;

public class UsuariosApiServicio : IUsuariosApiServicio
{
    private readonly IApiSoporteCliente _api;

    public UsuariosApiServicio(IApiSoporteCliente api)
    {
        _api = api;
    }

    public async Task<ResultadoOperacion<IReadOnlyList<UsuarioDto>>> ListarClientesAsync() =>
        await _api.GetAsync<IReadOnlyList<UsuarioDto>>("api/usuarios/clientes");

    public async Task<ResultadoOperacion<IReadOnlyList<UsuarioDto>>> ListarAgentesAsync() =>
        await _api.GetAsync<IReadOnlyList<UsuarioDto>>("api/usuarios?RolId=2");

    public async Task<ResultadoOperacion<IReadOnlyList<UsuarioDto>>> ListarAsync(FiltroUsuariosDto filtro)
    {
        var parametros = new Dictionary<string, string?>
        {
            [nameof(FiltroUsuariosDto.Texto)] = string.IsNullOrWhiteSpace(filtro.Texto) ? null : filtro.Texto.Trim(),
            [nameof(FiltroUsuariosDto.RolId)] = filtro.RolId?.ToString()
        };
        return await _api.GetAsync<IReadOnlyList<UsuarioDto>>(
            QueryHelpers.AddQueryString("api/usuarios", parametros.Where(p => p.Value is not null)));
    }

    public async Task<ResultadoOperacion<UsuarioDto>> CambiarRolAsync(int usuarioId, int rolId) =>
        await _api.PutJsonAsync<UsuarioDto>($"api/usuarios/{usuarioId}/rol", new CambiarRolDto { RolId = rolId });
}
