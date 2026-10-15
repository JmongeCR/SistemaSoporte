namespace Soporte.Abstracciones.Interfaces.Infraestructura;

/// <summary>Envío de correos. En desarrollo se guardan como archivos; en producción se enviaría por SMTP.</summary>
public interface IEnvioCorreo
{
    Task EnviarAsync(string destinatario, string asunto, string cuerpoHtml);
}
