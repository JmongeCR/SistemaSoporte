namespace Soporte.Abstracciones.Interfaces.Reglas;

public interface ISeguridadContrasenas
{
    string GenerarHash(string contrasena);
    bool Verificar(string contrasena, string hash);

    /// <summary>HU-02: genera un token aleatorio y su hash (solo el hash se persiste).</summary>
    (string Token, string TokenHash) GenerarTokenRecuperacion();
    string HashToken(string token);
}
