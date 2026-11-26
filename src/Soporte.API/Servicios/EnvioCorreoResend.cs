using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Soporte.Abstracciones.Interfaces.Infraestructura;

namespace Soporte.API.Servicios;

/// <summary>
/// Envío de correos mediante la API de Resend (https://resend.com).
/// Configuración requerida en user-secrets:
///   Correo:ResendApiKey   → API key de Resend
///   Correo:RemitenteDe    → dirección remitente (ej. "Soporte &lt;noreply@tudominio.com&gt;")
/// </summary>
public class EnvioCorreoResend : IEnvioCorreo
{
    private readonly HttpClient _http;
    private readonly string _apiKey;
    private readonly string _de;
    private readonly ILogger<EnvioCorreoResend> _logger;

    public EnvioCorreoResend(string apiKey, string de, ILogger<EnvioCorreoResend> logger)
    {
        _apiKey = apiKey;
        _de     = de;
        _logger = logger;
        _http   = new HttpClient { BaseAddress = new Uri("https://api.resend.com") };
        _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
    }

    public async Task EnviarAsync(string destinatario, string asunto, string cuerpoHtml)
    {
        var payload = JsonSerializer.Serialize(new
        {
            from    = _de,
            to      = new[] { destinatario },
            subject = asunto,
            html    = cuerpoHtml,
        });

        using var content = new StringContent(payload, Encoding.UTF8, "application/json");
        using var respuesta = await _http.PostAsync("/emails", content);

        if (respuesta.IsSuccessStatusCode)
        {
            _logger.LogInformation("Correo \"{Asunto}\" enviado a {Destinatario} via Resend", asunto, destinatario);
            return;
        }

        var error = await respuesta.Content.ReadAsStringAsync();
        _logger.LogError("Resend rechazó el correo \"{Asunto}\" para {Destinatario}: {Status} {Error}",
            asunto, destinatario, (int)respuesta.StatusCode, error);
    }
}
