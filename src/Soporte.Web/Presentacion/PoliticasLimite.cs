namespace Soporte.Web.Presentacion;

/// <summary>Nombres de las políticas de limitación de peticiones registradas en Program.cs.</summary>
public static class PoliticasLimite
{
    /// <summary>Intentos de login y de recuperación de contraseña por dirección IP.</summary>
    public const string Intentos = "intentos-cuenta";
}
