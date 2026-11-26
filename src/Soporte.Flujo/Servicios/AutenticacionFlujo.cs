using System.Net;
using Soporte.Abstracciones.DTOs;
using Soporte.Abstracciones.Interfaces.DA;
using Soporte.Abstracciones.Interfaces.Flujo;
using Soporte.Abstracciones.Interfaces.Infraestructura;
using Soporte.Abstracciones.Interfaces.Reglas;
using Soporte.Abstracciones.Modelos;

namespace Soporte.Flujo.Servicios;

public class AutenticacionFlujo : IAutenticacionFlujo
{
    private const string CredencialesInvalidas = "Correo o contraseña incorrectos.";
    private const string MensajeRecuperacion =
        "Si el correo está registrado, recibirás un enlace para restablecer tu contraseña. Revisá tu bandeja de entrada.";

    private readonly IUsuarioRepository _usuarios;
    private readonly ITokenRecuperacionRepository _tokens;
    private readonly ISeguridadContrasenas _seguridad;
    private readonly IGeneradorToken _generadorToken;
    private readonly IEnvioCorreo _correo;
    private readonly ConfiguracionRecuperacionDto _configuracion;
    private readonly IUnidadDeTrabajo _udt;

    /// <summary>Hash de relleno: se verifica igual cuando el correo no existe, para que el tiempo de respuesta no lo delate.</summary>
    private readonly Lazy<string> _hashDeRelleno;

    public AutenticacionFlujo(
        IUsuarioRepository usuarios,
        ITokenRecuperacionRepository tokens,
        ISeguridadContrasenas seguridad,
        IGeneradorToken generadorToken,
        IEnvioCorreo correo,
        ConfiguracionRecuperacionDto configuracion,
        IUnidadDeTrabajo udt)
    {
        _usuarios = usuarios;
        _tokens = tokens;
        _seguridad = seguridad;
        _generadorToken = generadorToken;
        _correo = correo;
        _configuracion = configuracion;
        _udt = udt;
        _hashDeRelleno = new Lazy<string>(() => _seguridad.GenerarHash(Guid.NewGuid().ToString()));
    }

    public async Task<ResultadoOperacion<SesionDto>> IniciarSesionAsync(LoginSolicitudDto dto)
    {
        var usuario = await _usuarios.ObtenerPorCorreoAsync(dto.Correo.Trim());

        // HU-01 esc. 2: el mismo mensaje para correo inexistente, cuenta inactiva o contraseña errónea.
        var contrasenaValida = _seguridad.Verificar(dto.Contrasena, usuario?.PasswordHash ?? _hashDeRelleno.Value);
        if (usuario is null || !usuario.Activo || !contrasenaValida)
            return ResultadoOperacion<SesionDto>.NoAutenticado(CredencialesInvalidas);

        var (token, expira) = _generadorToken.Generar(usuario);

        return ResultadoOperacion<SesionDto>.Ok(new SesionDto
        {
            UsuarioId = usuario.Id,
            Nombre = usuario.Nombre,
            Correo = usuario.Correo,
            RolId = usuario.RolId,
            Rol = usuario.Rol!.Nombre,
            Token = token,
            Expira = expira
        });
    }

    public async Task<ResultadoOperacion> SolicitarRecuperacionAsync(SolicitudRecuperacionDto dto)
    {
        var usuario = await _usuarios.ObtenerPorCorreoAsync(dto.Correo.Trim());

        // HU-02 esc. 2: la respuesta es idéntica exista o no el correo.
        if (usuario is { Activo: true })
        {
            var (token, tokenHash) = _seguridad.GenerarTokenRecuperacion();
            var ahora = DateTime.UtcNow;

            await _tokens.CrearAsync(new TokenRecuperacion
            {
                UsuarioId = usuario.Id,
                TokenHash = tokenHash,
                FechaCreacion = ahora,
                FechaExpiracion = ahora.AddHours(TokenRecuperacion.HorasVigencia)
            });

            var enlace = $"{_configuracion.UrlRestablecer}?token={Uri.EscapeDataString(token)}";
            var nombre  = WebUtility.HtmlEncode(usuario.Nombre);
            var urlSafe = WebUtility.HtmlEncode(enlace);
            await _correo.EnviarAsync(
                usuario.Correo,
                "Restablecé tu contraseña — Sistema de Soporte",
                $"""
                <p style="font-size:15px;color:#1e3a5f;margin:0 0 10px">
                  Hola, <strong>{nombre}</strong>:
                </p>
                <p style="font-size:14px;color:#4a5568;line-height:1.7;margin:0 0 24px">
                  Recibimos una solicitud para restablecer la contraseña de tu cuenta
                  en el <strong style="color:#1e3a5f">Sistema de Soporte Técnico</strong>.
                  Si fuiste vos, hacé clic en el botón a continuación para crear una nueva contraseña.
                  Si no realizaste esta solicitud, podés ignorar este mensaje.
                </p>
                <table width="100%" cellpadding="0" cellspacing="0" role="presentation"
                       style="margin:0 0 28px">
                  <tr>
                    <td align="center">
                      <a href="{urlSafe}"
                         style="display:inline-block;background-color:#1e3a5f;color:#ffffff;
                                font-size:14px;font-weight:700;text-decoration:none;
                                padding:13px 40px;border-radius:4px;letter-spacing:.4px">
                        Restablecer contraseña
                      </a>
                    </td>
                  </tr>
                </table>
                <p style="font-size:12px;color:#718096;margin:0 0 6px">
                  Este enlace vence en <strong style="color:#1e3a5f">{TokenRecuperacion.HorasVigencia}&nbsp;hora(s)</strong>.
                </p>
                <p style="font-size:12px;color:#718096;margin:0">
                  Si el botón no funciona, copiá y pegá este enlace en tu navegador:<br/>
                  <a href="{urlSafe}" style="color:#1e3a5f;word-break:break-all">{urlSafe}</a>
                </p>
                """);
        }

        return ResultadoOperacion.Ok(MensajeRecuperacion);
    }

    public async Task<ResultadoOperacion> RestablecerContrasenaAsync(RestablecerContrasenaDto dto)
    {
        var token = await _tokens.ObtenerPorHashAsync(_seguridad.HashToken(dto.Token));

        // HU-02 esc. 3: enlace inexistente, ya usado o con más de 24 horas.
        if (token is null || token.Usado || token.FechaExpiracion < DateTime.UtcNow)
            return ResultadoOperacion.Fallo("El enlace no es válido o ya expiró. Solicitá uno nuevo.");

        // Operación atómica: marcar token como usado y actualizar contraseña en la misma transacción.
        // Si dos solicitudes concurrentes llegan con el mismo token, solo la primera marcará Usado=true;
        // la segunda recibirá false de MarcarUsadoSiDisponibleAsync y retorna error.
        return await _udt.EjecutarAsync(async () =>
        {
            var marcado = await _tokens.MarcarUsadoSiDisponibleAsync(token.Id);
            if (!marcado)
                return ResultadoOperacion.Fallo("El enlace no es válido o ya expiró. Solicitá uno nuevo.");

            await _usuarios.ActualizarPasswordAsync(token.UsuarioId, _seguridad.GenerarHash(dto.NuevaContrasena));
            return ResultadoOperacion.Ok("Tu contraseña se actualizó. Ya podés iniciar sesión.");
        });
    }
}
