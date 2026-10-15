namespace Soporte.Abstracciones.DTOs;

/// <summary>Elemento genérico de catálogo (categorías, prioridades, estados, roles) para listas desplegables.</summary>
public class CatalogoDto
{
    public int Id { get; init; }
    public string Nombre { get; init; } = string.Empty;
}
