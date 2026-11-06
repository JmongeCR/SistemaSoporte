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
    private readonly ISeguridadContrasenas _seguridad;

    public UsuarioFlujo(IUsuarioRepository usuarios, ICatalogoRepository catalogos, IUsuarioReglas reglas, ISeguridadContrasenas seguridad)
    {
        _usuarios = usuarios;
        _catalogos = catalogos;
        _reglas = reglas;
        _seguridad = seguridad;
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

    public async Task<ResultadoOperacion<UsuarioDto>> ActualizarPerfilAsync(int usuarioId, ActualizarPerfilDto dto, UsuarioContextoDto usuarioActual)
    {
        if (usuarioActual.UsuarioId != usuarioId)
            return ResultadoOperacion<UsuarioDto>.SinPermiso("Solo podés editar tu propio perfil.");

        var usuario = await _usuarios.ObtenerPorIdAsync(usuarioId);
        if (usuario is null)
            return ResultadoOperacion<UsuarioDto>.NoEncontrado($"No existe el usuario #{usuarioId}.");

        if (!string.IsNullOrWhiteSpace(dto.NuevaContrasena))
        {
            if (string.IsNullOrWhiteSpace(dto.ContrasenaActual))
                return ResultadoOperacion<UsuarioDto>.Fallo("Ingresá tu contraseña actual para poder cambiarla.");

            if (!_seguridad.Verificar(dto.ContrasenaActual, usuario.PasswordHash))
                return ResultadoOperacion<UsuarioDto>.Fallo("La contraseña actual es incorrecta.");

            await _usuarios.ActualizarPasswordAsync(usuarioId, _seguridad.GenerarHash(dto.NuevaContrasena));
        }

        var nombreTrimmed = dto.Nombre.Trim();
        if (usuario.Nombre != nombreTrimmed)
            await _usuarios.ActualizarNombreAsync(usuarioId, nombreTrimmed);

        var actualizado = await _usuarios.ObtenerPorIdAsync(usuarioId);
        var roles = await _catalogos.ListarRolesAsync();
        var rol = roles.SingleOrDefault(r => r.Id == actualizado!.RolId);

        return ResultadoOperacion<UsuarioDto>.Ok(new UsuarioDto
        {
            Id = actualizado!.Id,
            Nombre = actualizado.Nombre,
            Correo = actualizado.Correo,
            RolId = actualizado.RolId,
            Rol = rol?.Nombre ?? string.Empty,
            Activo = actualizado.Activo
        }, "Perfil actualizado correctamente.");
    }
}
