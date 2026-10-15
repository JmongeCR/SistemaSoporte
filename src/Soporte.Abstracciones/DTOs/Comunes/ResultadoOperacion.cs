using Soporte.Abstracciones.Enumeraciones;

namespace Soporte.Abstracciones.DTOs;

/// <summary>Resultado de una operación de negocio: éxito, o tipo de error y mensaje para el usuario.</summary>
public class ResultadoOperacion
{
    public bool Exito { get; init; }
    public TipoError TipoError { get; init; }
    public string? Mensaje { get; init; }
    public IReadOnlyList<string> Errores { get; init; } = Array.Empty<string>();

    public static ResultadoOperacion Ok(string? mensaje = null) => new() { Exito = true, Mensaje = mensaje };

    public static ResultadoOperacion Fallo(string mensaje, params string[] errores) =>
        new() { TipoError = TipoError.Validacion, Mensaje = mensaje, Errores = errores };

    public static ResultadoOperacion NoEncontrado(string mensaje) =>
        new() { TipoError = TipoError.NoEncontrado, Mensaje = mensaje };

    public static ResultadoOperacion SinPermiso(string mensaje) =>
        new() { TipoError = TipoError.SinPermiso, Mensaje = mensaje };

    public static ResultadoOperacion Conflicto(string mensaje) =>
        new() { TipoError = TipoError.Conflicto, Mensaje = mensaje };

    public static ResultadoOperacion NoAutenticado(string mensaje) =>
        new() { TipoError = TipoError.NoAutenticado, Mensaje = mensaje };
}

public class ResultadoOperacion<T> : ResultadoOperacion
{
    public T? Datos { get; init; }

    public static ResultadoOperacion<T> Ok(T datos, string? mensaje = null) =>
        new() { Exito = true, Datos = datos, Mensaje = mensaje };

    /// <summary>Propaga un fallo (por ejemplo, de una regla de negocio) conservando su tipo y mensaje.</summary>
    public static ResultadoOperacion<T> Desde(ResultadoOperacion fallo) =>
        new() { TipoError = fallo.TipoError, Mensaje = fallo.Mensaje, Errores = fallo.Errores };

    public new static ResultadoOperacion<T> Fallo(string mensaje, params string[] errores) =>
        Desde(ResultadoOperacion.Fallo(mensaje, errores));

    public new static ResultadoOperacion<T> NoEncontrado(string mensaje) =>
        Desde(ResultadoOperacion.NoEncontrado(mensaje));

    public new static ResultadoOperacion<T> SinPermiso(string mensaje) =>
        Desde(ResultadoOperacion.SinPermiso(mensaje));

    public new static ResultadoOperacion<T> Conflicto(string mensaje) =>
        Desde(ResultadoOperacion.Conflicto(mensaje));

    public new static ResultadoOperacion<T> NoAutenticado(string mensaje) =>
        Desde(ResultadoOperacion.NoAutenticado(mensaje));
}
