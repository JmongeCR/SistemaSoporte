using Soporte.Abstracciones.DTOs;
using Soporte.Abstracciones.Enumeraciones;
using Soporte.Abstracciones.Interfaces.DA;
using Soporte.Abstracciones.Interfaces.Flujo;
using Soporte.Abstracciones.Interfaces.Reglas;

namespace Soporte.Flujo.Servicios;

public class UsuarioFlujo : IUsuarioFlujo
{
    private const string SinPermisoUsuarios = "Solo un administrador puede gestionar usuarios y roles.";

    private readonly IUsuarioRepository _usuarios;
    private readonly ICatalogoRepository _catalogos;
    private readonly IUsuarioReglas _reglas;

    public UsuarioFlujo(IUsuarioRepository usuarios, ICatalogoRepository catalogos, IUsuarioReglas reglas)
    {
        _usuarios = usuarios;
        _catalogos = catalogos;
        _reglas = reglas;
    }

    public async Task<ResultadoOperacion<IReadOnlyList<UsuarioDto>>> ListarAsync(FiltroUsuariosDto filtro, UsuarioContextoDto usuario)
    {
        if (!_reglas.PuedeAdministrarUsuarios(usuario))
            return ResultadoOperacion<IReadOnlyList<UsuarioDto>>.SinPermiso(SinPermisoUsuarios);

        return ResultadoOperacion<IReadOnlyList<UsuarioDto>>.Ok(await _usuarios.ListarAsync(filtro));
    }

    public async Task<ResultadoOperacion<IReadOnlyList<UsuarioDto>>> BuscarClientesAsync(string? texto, UsuarioContextoDto usuario)
    {
        if (!_reglas.PuedeBuscarClientes(usuario))
            return ResultadoOperacion<IReadOnlyList<UsuarioDto>>.SinPermiso("Solo el equipo de soporte puede elegir el cliente solicitante.");

        var clientes = await _usuarios.ListarAsync(new FiltroUsuariosDto { Texto = texto, RolId = (int)RolTipo.Cliente });
        return ResultadoOperacion<IReadOnlyList<UsuarioDto>>.Ok(clientes.Where(c => c.Activo).ToList());
    }

    public async Task<ResultadoOperacion<UsuarioDto>> CambiarRolAsync(int usuarioId, CambiarRolDto dto, UsuarioContextoDto usuario)
    {
        if (!_reglas.PuedeAdministrarUsuarios(usuario))
            return ResultadoOperacion<UsuarioDto>.SinPermiso(SinPermisoUsuarios);

        var objetivo = await _usuarios.ObtenerPorIdAsync(usuarioId);
        if (objetivo is null)
            return ResultadoOperacion<UsuarioDto>.NoEncontrado($"No existe el usuario #{usuarioId}.");

        if (objetivo.RolId == dto.RolId)
        {
            var mismoRol = (await _catalogos.ListarRolesAsync()).Single(r => r.Id == dto.RolId);
            return ResultadoOperacion<UsuarioDto>.Ok(new UsuarioDto
            {
                Id = objetivo.Id, Nombre = objetivo.Nombre, Correo = objetivo.Correo,
                RolId = mismoRol.Id, Rol = mismoRol.Nombre, Activo = objetivo.Activo
            }, "El usuario ya tenía ese rol.");
        }

        var validacion = await _reglas.ValidarCambioRolAsync(objetivo, dto.RolId);
        if (!validacion.Exito)
            return ResultadoOperacion<UsuarioDto>.Desde(validacion);

        await _usuarios.ActualizarRolAsync(usuarioId, dto.RolId);

        var rol = (await _catalogos.ListarRolesAsync()).Single(r => r.Id == dto.RolId);
        return ResultadoOperacion<UsuarioDto>.Ok(new UsuarioDto
        {
            Id = objetivo.Id,
            Nombre = objetivo.Nombre,
            Correo = objetivo.Correo,
            RolId = rol.Id,
            Rol = rol.Nombre,
            Activo = objetivo.Activo
        }, "Rol actualizado correctamente.");
    }
}
