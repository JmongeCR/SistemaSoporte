using System.Net;
using System.Text.RegularExpressions;
using Soporte.Abstracciones.Interfaces.Infraestructura;

namespace Soporte.API.Servicios;

/// <summary>
/// Envío de correos para desarrollo: cada correo se guarda como archivo .html en una carpeta local
/// (appsettings.json → Correo:CarpetaSalida) y se registra en el log. Para producción se implementaría
/// IEnvioCorreo con SMTP sin cambiar el resto del sistema.
/// </summary>
public class EnvioCorreoArchivo : IEnvioCorreo
{
    private readonly string _carpeta;
    private readonly ILogger<EnvioCorreoArchivo> _logger;

    public EnvioCorreoArchivo(string carpeta, ILogger<EnvioCorreoArchivo> logger)
    {
        _carpeta = carpeta;
        _logger = logger;
    }

    public async Task EnviarAsync(string destinatario, string asunto, string cuerpoHtml)
    {
        Directory.CreateDirectory(_carpeta);

        var nombre = $"{DateTime.UtcNow:yyyyMMdd-HHmmss}-{Regex.Replace(destinatario, "[^a-zA-Z0-9]", "_")}.html";
        var ruta = Path.Combine(_carpeta, nombre);

        var contenido = $"""
            <!DOCTYPE html>
            <html lang="es"><head><meta charset="utf-8"><title>{WebUtility.HtmlEncode(asunto)}</title></head>
            <body style="font-family: system-ui, sans-serif; max-width: 560px; margin: 32px auto;">
            <p style="color:#737A87; font-size:13px;">Para: {WebUtility.HtmlEncode(destinatario)}<br>Asunto: {WebUtility.HtmlEncode(asunto)}</p>
            <hr>
            {cuerpoHtml}
            </body></html>
            """;

        await File.WriteAllTextAsync(ruta, contenido);
        _logger.LogInformation("Correo \"{Asunto}\" para {Destinatario} guardado en {Ruta}", asunto, destinatario, ruta);
    }
}
