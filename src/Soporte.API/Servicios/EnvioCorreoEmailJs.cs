using System.Text;
using System.Text.Json;
using Soporte.Abstracciones.Interfaces.Infraestructura;

namespace Soporte.API.Servicios;

/// <summary>
/// Envío de correos mediante la API REST de EmailJS (https://emailjs.com).
/// Configuración requerida en user-secrets:
///   EmailJs:ServiceId   → ID del servicio en EmailJS
///   EmailJs:TemplateId  → ID de la plantilla en EmailJS
///   EmailJs:PublicKey   → Clave pública (user_id)
///   EmailJs:PrivateKey  → Clave privada (accessToken)
/// La plantilla debe usar las variables: {{to_email}}, {{subject}}, {{message}}
/// </summary>
public class EnvioCorreoEmailJs : IEnvioCorreo
{
    private readonly HttpClient _http;
    private readonly string _serviceId;
    private readonly string _templateId;
    private readonly string _publicKey;
    private readonly string _privateKey;
    private readonly ILogger<EnvioCorreoEmailJs> _logger;

    public EnvioCorreoEmailJs(
        string serviceId, string templateId,
        string publicKey, string privateKey,
        ILogger<EnvioCorreoEmailJs> logger)
    {
        _serviceId  = serviceId;
        _templateId = templateId;
        _publicKey  = publicKey;
        _privateKey = privateKey;
        _logger     = logger;
        _http       = new HttpClient { BaseAddress = new Uri("https://api.emailjs.com") };
    }

    public async Task EnviarAsync(string destinatario, string asunto, string cuerpoHtml)
    {
        var payload = JsonSerializer.Serialize(new
        {
            service_id    = _serviceId,
            template_id   = _templateId,
            user_id       = _publicKey,
            accessToken   = _privateKey,
            template_params = new
            {
                to_email = destinatario,
                subject  = asunto,
                message  = cuerpoHtml,
            },
        });

        using var content  = new StringContent(payload, Encoding.UTF8, "application/json");
        using var respuesta = await _http.PostAsync("/api/v1.0/email/send", content);

        if (respuesta.IsSuccessStatusCode)
        {
            _logger.LogInformation("Correo \"{Asunto}\" enviado a {Destinatario} via EmailJS", asunto, destinatario);
            return;
        }

        var error = await respuesta.Content.ReadAsStringAsync();
        _logger.LogError("EmailJS rechazó el correo \"{Asunto}\" para {Destinatario}: {Status} {Error}",
            asunto, destinatario, (int)respuesta.StatusCode, error);
    }
}
