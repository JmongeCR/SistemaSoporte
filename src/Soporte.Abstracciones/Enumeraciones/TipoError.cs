namespace Soporte.Abstracciones.Enumeraciones;

/// <summary>Clasifica el fallo de una operación para que la API elija el código HTTP.</summary>
public enum TipoError
{
    Ninguno = 0,
    Validacion = 1,
    NoEncontrado = 2,
    SinPermiso = 3,
    Conflicto = 4,
    NoAutenticado = 5
}
