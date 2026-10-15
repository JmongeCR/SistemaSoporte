using System.ComponentModel.DataAnnotations;

namespace Soporte.Abstracciones.Modelos;

public class Categoria
{
    public int Id { get; set; }

    [Required, StringLength(50)]
    public string Nombre { get; set; } = string.Empty;

    public bool Activa { get; set; } = true;
}
