namespace Soporte.Abstracciones.DTOs;

/// <summary>Datos de configuración (appsettings.json) que necesita el flujo de recuperación de contraseña.</summary>
public class ConfiguracionRecuperacionDto
{
    /// <summary>Página de Soporte.Web que recibe el token, ej. http://localhost:5122/Cuenta/Restablecer</summary>
    public string UrlRestablecer { get; init; } = string.Empty;
}
