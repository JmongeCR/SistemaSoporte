using System.ComponentModel.DataAnnotations;

namespace Soporte.Abstracciones.Modelos;

/// <summary>Token de restablecimiento de contraseña, válido 24 horas (HU-02).</summary>
public class TokenRecuperacion
{
    public const int HorasVigencia = 24;

    public int Id { get; set; }
    public int UsuarioId { get; set; }

    /// <summary>Se guarda el hash del token, nunca el token en claro.</summary>
    [Required, StringLength(128)]
    public string TokenHash { get; set; } = string.Empty;

    public DateTime FechaCreacion { get; set; }
    public DateTime FechaExpiracion { get; set; }
    public bool Usado { get; set; }

    public Usuario? Usuario { get; set; }
}
