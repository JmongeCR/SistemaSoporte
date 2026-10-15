namespace Soporte.API.Servicios;

/// <summary>Sección "Jwt" de appsettings.json. La Clave va en user-secrets, nunca en el repositorio.</summary>
public class ConfiguracionJwt
{
    public const int LongitudMinimaClave = 32;

    public string Emisor { get; init; } = string.Empty;
    public string Audiencia { get; init; } = string.Empty;
    public string Clave { get; init; } = string.Empty;
    public int MinutosExpiracion { get; init; } = 480;
}
