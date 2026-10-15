using System.Security.Cryptography;
using System.Text;
using Soporte.Abstracciones.Interfaces.Reglas;

namespace Soporte.Reglas.Reglas;

/// <summary>
/// Hash de contraseñas con PBKDF2-SHA256 (incluido en .NET, sin paquetes externos).
/// Formato almacenado: PBKDF2-SHA256$iteraciones$salBase64$hashBase64
/// </summary>
public class SeguridadContrasenas : ISeguridadContrasenas
{
    private const string Algoritmo = "PBKDF2-SHA256";
    private const int Iteraciones = 100_000;
    private const int TamanoSal = 16;
    private const int TamanoHash = 32;
    private const int TamanoToken = 32;

    public string GenerarHash(string contrasena)
    {
        var sal = RandomNumberGenerator.GetBytes(TamanoSal);
        var hash = Rfc2898DeriveBytes.Pbkdf2(contrasena, sal, Iteraciones, HashAlgorithmName.SHA256, TamanoHash);
        return $"{Algoritmo}${Iteraciones}${Convert.ToBase64String(sal)}${Convert.ToBase64String(hash)}";
    }

    public bool Verificar(string contrasena, string hash)
    {
        var partes = hash.Split('$');
        if (partes.Length != 4 || partes[0] != Algoritmo || !int.TryParse(partes[1], out var iteraciones))
            return false;

        var sal = Convert.FromBase64String(partes[2]);
        var esperado = Convert.FromBase64String(partes[3]);
        var calculado = Rfc2898DeriveBytes.Pbkdf2(contrasena, sal, iteraciones, HashAlgorithmName.SHA256, esperado.Length);
        return CryptographicOperations.FixedTimeEquals(calculado, esperado);
    }

    public (string Token, string TokenHash) GenerarTokenRecuperacion()
    {
        var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(TamanoToken))
            .Replace('+', '-').Replace('/', '_').TrimEnd('=');
        return (token, HashToken(token));
    }

    public string HashToken(string token) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
}
