using System.Security.Claims;
using Soporte.Abstracciones.DTOs;
using Soporte.Abstracciones.Enumeraciones;
using Soporte.Abstracciones.Interfaces.DA;
using Soporte.Abstracciones.Modelos;
using Soporte.API.Servicios;
using Soporte.Flujo.Servicios;
using Soporte.Pruebas.Dobles;
using Soporte.Reglas.Reglas;

namespace Soporte.Pruebas.Flujo;

/// <summary>HU-14 a HU-16: usuarios, cambio de rol y matriz de permisos.</summary>
public class RolesPruebas
{
    /// <summary>Usuarios en memoria: el Id 1 es Administrador; se puede agregar un segundo.</summary>
    private sealed class UsuariosEnMemoria : IUsuarioRepository
    {
        public List<Usuario> Usuarios { get; } = new()
        {
            new() { Id = 1, Nombre = "Ana Rodríguez", Correo = "admin1@soporte.test", RolId = 1, Activo = true },
            new() { Id = 3, Nombre = "Luis Vargas", Correo = "agente1@soporte.test", RolId = 2, Activo = true },
            new() { Id = 5, Nombre = "Sofía Castro", Correo = "cliente1@soporte.test", RolId = 3, Activo = true }
        };
        public int Escrituras { get; private set; }

        public Task<Usuario?> ObtenerPorIdAsync(int id) => Task.FromResult(Usuarios.SingleOrDefault(u => u.Id == id));
        public Task<int> ContarPorRolAsync(int rolId) => Task.FromResult(Usuarios.Count(u => u.RolId == rolId && u.Activo));
        public Task ActualizarRolAsync(int usuarioId, int rolId)
        {
            Escrituras++;
            Usuarios.Single(u => u.Id == usuarioId).RolId = rolId;
            return Task.CompletedTask;
        }
        public Task<IReadOnlyList<UsuarioDto>> ListarAsync(FiltroUsuariosDto filtro) =>
            Task.FromResult<IReadOnlyList<UsuarioDto>>(Usuarios.Select(u => new UsuarioDto { Id = u.Id, Nombre = u.Nombre, RolId = u.RolId }).ToList());
        public Task<Usuario?> ObtenerPorCorreoAsync(string correo) => throw new NotSupportedException();
        public Task ActualizarPasswordAsync(int usuarioId, string passwordHash) => throw new NotSupportedException();
    }

    private sealed class CatalogoConRoles : ICatalogoRepository
    {
        public Task<IReadOnlyList<CatalogoDto>> ListarRolesAsync() => Task.FromResult<IReadOnlyList<CatalogoDto>>(new[]
        {
            new CatalogoDto { Id = 1, Nombre = "Administrador" }, new CatalogoDto { Id = 2, Nombre = "Agente" }, new CatalogoDto { Id = 3, Nombre = "Cliente" }
        });
        public Task<IReadOnlyList<Permiso>> ListarPermisosConRolesAsync() => Task.FromResult<IReadOnlyList<Permiso>>(new[]
        {
            new Permiso { Id = 1, Codigo = "VER_TODOS_TICKETS", Descripcion = "Ver todos", Grupo = "Visibilidad", Roles = { new RolPermiso { RolId = 1 } } },
            new Permiso { Id = 2, Codigo = "CREAR_TICKET", Descripcion = "Crear", Grupo = "Gestión de tickets", Roles = { new RolPermiso { RolId = 1 }, new RolPermiso { RolId = 3 } } }
        });
        public Task<IReadOnlyList<CatalogoDto>> ListarCategoriasAsync() => throw new NotSupportedException();
        public Task<IReadOnlyList<CatalogoDto>> ListarPrioridadesAsync() => throw new NotSupportedException();
        public Task<IReadOnlyList<CatalogoDto>> ListarEstadosAsync() => throw new NotSupportedException();
        public Task<bool> ExisteCategoriaActivaAsync(int categoriaId) => throw new NotSupportedException();
    }

    private readonly UsuariosEnMemoria _usuarios = new();
    private UsuarioFlujo Flujo() => new(_usuarios, new CatalogoConRoles(), new UsuarioReglas(_usuarios));

    [Fact]
    public async Task No_se_puede_quitar_el_rol_al_unico_administrador()
    {
        var resultado = await Flujo().CambiarRolAsync(1, new CambiarRolDto { RolId = 3 }, Datos.Admin);

        Assert.Equal(TipoError.Validacion, resultado.TipoError);
        Assert.Contains("único usuario con rol Administrador", resultado.Mensaje);
        Assert.Equal(0, _usuarios.Escrituras);
    }

    [Fact]
    public async Task Con_dos_administradores_se_puede_quitar_el_rol_a_uno()
    {
        _usuarios.Usuarios.Add(new Usuario { Id = 2, Nombre = "Carlos Mora", RolId = 1, Activo = true });

        var resultado = await Flujo().CambiarRolAsync(2, new CambiarRolDto { RolId = 2 }, Datos.Admin);

        Assert.True(resultado.Exito);
        Assert.Equal("Agente", resultado.Datos!.Rol);
    }

    [Fact]
    public async Task Asignar_el_mismo_rol_no_escribe_en_la_base()
    {
        var resultado = await Flujo().CambiarRolAsync(3, new CambiarRolDto { RolId = 2 }, Datos.Admin);

        Assert.True(resultado.Exito);
        Assert.Equal(0, _usuarios.Escrituras);
    }

    [Theory]
    [InlineData(3, RolTipo.Agente)]
    [InlineData(5, RolTipo.Cliente)]
    public async Task Solo_el_administrador_lista_usuarios_y_cambia_roles(int usuarioId, RolTipo rol)
    {
        var usuario = new UsuarioContextoDto { UsuarioId = usuarioId, Rol = rol };

        Assert.Equal(TipoError.SinPermiso, (await Flujo().ListarAsync(new FiltroUsuariosDto(), usuario)).TipoError);
        Assert.Equal(TipoError.SinPermiso, (await Flujo().CambiarRolAsync(1, new CambiarRolDto { RolId = 3 }, usuario)).TipoError);
        Assert.Equal(0, _usuarios.Escrituras);
    }

    [Fact]
    public async Task La_matriz_de_permisos_es_solo_para_el_administrador()
    {
        var flujo = new CatalogoFlujo(new CatalogoConRoles());

        Assert.Equal(TipoError.SinPermiso, (await flujo.ObtenerMatrizPermisosAsync(3, Datos.Sofia)).TipoError);
        Assert.Equal(TipoError.SinPermiso, (await flujo.ObtenerMatrizPermisosAsync(3, Datos.Luis)).TipoError);

        var matriz = await flujo.ObtenerMatrizPermisosAsync(3, Datos.Admin);
        Assert.True(matriz.Exito);
        var permisos = matriz.Datos!.Grupos.SelectMany(g => g.Permisos).ToList();
        Assert.False(permisos.Single(p => p.Codigo == "VER_TODOS_TICKETS").Concedido);
        Assert.True(permisos.Single(p => p.Codigo == "CREAR_TICKET").Concedido);
    }

    [Fact]
    public async Task Un_token_con_el_rol_anterior_deja_de_ser_valido()
    {
        var validador = new ValidadorSesion(_usuarios);
        ClaimsPrincipal Token(int id, string rol) =>
            new(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, id.ToString()),
                new Claim(ClaimTypes.Role, rol),
                new Claim("cv", "0")   // versión de contraseña vigente al emitir el token
            }, "prueba"));

        Assert.True(await validador.EsVigenteAsync(Token(3, "Agente")));

        await _usuarios.ActualizarRolAsync(3, (int)RolTipo.Cliente);   // el administrador le cambió el rol

        Assert.False(await validador.EsVigenteAsync(Token(3, "Agente")));
        Assert.True(await validador.EsVigenteAsync(Token(3, "Cliente")));
    }
}
