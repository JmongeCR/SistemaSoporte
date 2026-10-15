using System.Security.Claims;
using Soporte.Abstracciones.DTOs;
using Soporte.Abstracciones.Interfaces.DA;
using Soporte.Abstracciones.Interfaces.Infraestructura;
using Soporte.Abstracciones.Modelos;
using Soporte.API.Servicios;
using Soporte.Flujo.Servicios;
using Soporte.Pruebas.Dobles;
using Soporte.Reglas.Reglas;

namespace Soporte.Pruebas.Flujo;

/// <summary>HU-02: restablecimiento de contraseña y HU-01: invalidación de JWT.</summary>
public class AutenticacionFlujoPruebas
{
    // ─── dobles ───────────────────────────────────────────────────────────────

    private sealed class UsuariosEnMemoria : IUsuarioRepository
    {
        public Usuario Usuario { get; } = new()
        {
            Id = 1,
            Nombre = "Ana Rodríguez",
            Correo = "ana@soporte.test",
            PasswordHash = "viejo_hash",
            RolId = 3,
            Activo = true,
            ContrasenaVersion = 0
        };

        public int ActualizacionesPassword { get; private set; }

        public Task<Usuario?> ObtenerPorIdAsync(int id) =>
            Task.FromResult(id == Usuario.Id ? Usuario : null);
        public Task<Usuario?> ObtenerPorCorreoAsync(string correo) =>
            Task.FromResult(correo == Usuario.Correo ? Usuario : null);
        public Task ActualizarPasswordAsync(int usuarioId, string passwordHash)
        {
            ActualizacionesPassword++;
            Usuario.PasswordHash = passwordHash;
            Usuario.ContrasenaVersion++;
            return Task.CompletedTask;
        }
        public Task<IReadOnlyList<UsuarioDto>> ListarAsync(FiltroUsuariosDto filtro) => throw new NotSupportedException();
        public Task<int> ContarPorRolAsync(int rolId) => throw new NotSupportedException();
        public Task ActualizarRolAsync(int usuarioId, int rolId) => throw new NotSupportedException();
    }

    private sealed class TokensEnMemoria : ITokenRecuperacionRepository
    {
        public TokenRecuperacion? Token { get; set; }
        public int MarcadosUsados { get; private set; }

        public Task CrearAsync(TokenRecuperacion token)
        {
            Token = token;
            return Task.CompletedTask;
        }
        public Task<TokenRecuperacion?> ObtenerPorHashAsync(string hash) =>
            Task.FromResult(Token?.TokenHash == hash ? Token : null);

        public Task MarcarUsadoAsync(int id)
        {
            if (Token?.Id == id) Token.Usado = true;
            return Task.CompletedTask;
        }

        public Task<bool> MarcarUsadoSiDisponibleAsync(int id)
        {
            if (Token is null || Token.Id != id || Token.Usado)
                return Task.FromResult(false);
            Token.Usado = true;
            MarcadosUsados++;
            return Task.FromResult(true);
        }
    }

    private sealed class CorreoDescartado : IEnvioCorreo
    {
        public Task EnviarAsync(string destinatario, string asunto, string cuerpoHtml) => Task.CompletedTask;
    }

    // ─── helpers ──────────────────────────────────────────────────────────────

    private readonly SeguridadContrasenas _seguridad = new();
    private readonly UsuariosEnMemoria _usuarios = new();
    private readonly TokensEnMemoria _tokensRepo = new();

    private AutenticacionFlujo Flujo() => new(
        _usuarios,
        _tokensRepo,
        _seguridad,
        new GeneradorTokenJwt(new ConfiguracionJwt { Clave = new string('x', 32), Emisor = "test", Audiencia = "test" }),
        new CorreoDescartado(),
        new ConfiguracionRecuperacionDto { UrlRestablecer = "http://localhost/Cuenta/Restablecer" },
        new UnidadDeTrabajoFalsa());

    private (string Token, string Hash) CrearTokenValido()
    {
        var (token, hash) = _seguridad.GenerarTokenRecuperacion();
        _tokensRepo.Token = new TokenRecuperacion
        {
            Id = 1,
            UsuarioId = _usuarios.Usuario.Id,
            TokenHash = hash,
            FechaCreacion = DateTime.UtcNow,
            FechaExpiracion = DateTime.UtcNow.AddHours(24),
            Usado = false
        };
        return (token, hash);
    }

    // ─── pruebas: restablecimiento ───────────────────────────────────────────

    [Fact]
    public async Task Token_valido_permite_restablecer_contrasena()
    {
        var (token, _) = CrearTokenValido();

        var resultado = await Flujo().RestablecerContrasenaAsync(new RestablecerContrasenaDto
        {
            Token = token,
            NuevaContrasena = "NuevaClave123!"
        });

        Assert.True(resultado.Exito);
        Assert.True(_tokensRepo.Token!.Usado);
        Assert.Equal(1, _usuarios.ActualizacionesPassword);
    }

    [Fact]
    public async Task Token_ya_usado_rechaza_el_restablecimiento()
    {
        var (token, _) = CrearTokenValido();
        _tokensRepo.Token!.Usado = true;

        var resultado = await Flujo().RestablecerContrasenaAsync(new RestablecerContrasenaDto
        {
            Token = token,
            NuevaContrasena = "NuevaClave123!"
        });

        Assert.False(resultado.Exito);
        Assert.Equal(0, _usuarios.ActualizacionesPassword);
    }

    [Fact]
    public async Task Token_expirado_rechaza_el_restablecimiento()
    {
        var (token, _) = CrearTokenValido();
        _tokensRepo.Token!.FechaExpiracion = DateTime.UtcNow.AddHours(-1);

        var resultado = await Flujo().RestablecerContrasenaAsync(new RestablecerContrasenaDto
        {
            Token = token,
            NuevaContrasena = "NuevaClave123!"
        });

        Assert.False(resultado.Exito);
        Assert.Equal(0, _usuarios.ActualizacionesPassword);
    }

    [Fact]
    public async Task Solicitudes_concurrentes_con_el_mismo_token_solo_una_cambia_la_clave()
    {
        var (token, _) = CrearTokenValido();
        var dto = new RestablecerContrasenaDto { Token = token, NuevaContrasena = "NuevaClave123!" };

        // Simula concurrencia: dos solicitudes llegan y la segunda llama al mismo token ya marcado.
        var primera = await Flujo().RestablecerContrasenaAsync(dto);
        var segunda = await Flujo().RestablecerContrasenaAsync(dto);

        Assert.True(primera.Exito);
        Assert.False(segunda.Exito);
        Assert.Equal(1, _tokensRepo.MarcadosUsados);
        Assert.Equal(1, _usuarios.ActualizacionesPassword);
    }

    // ─── pruebas: invalidación de JWT al cambiar contraseña ─────────────────

    [Fact]
    public async Task JWT_previo_al_cambio_de_contrasena_deja_de_ser_valido()
    {
        var validador = new ValidadorSesion(_usuarios);

        ClaimsPrincipal TokenCon(int id, string rol, int version) =>
            new(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, id.ToString()),
                new Claim(ClaimTypes.Role, rol),
                new Claim("cv", version.ToString())
            }, "prueba"));

        // Token emitido antes del cambio de contraseña (versión 0).
        Assert.True(await validador.EsVigenteAsync(TokenCon(1, "Cliente", 0)));

        // Simula cambio de contraseña: versión del usuario sube a 1.
        await _usuarios.ActualizarPasswordAsync(1, "nuevo_hash");

        Assert.False(await validador.EsVigenteAsync(TokenCon(1, "Cliente", 0)));
        Assert.True(await validador.EsVigenteAsync(TokenCon(1, "Cliente", 1)));
    }
}
