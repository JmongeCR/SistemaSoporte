namespace Soporte.Abstracciones.Interfaces.DA;

/// <summary>
/// Ejecuta varias operaciones de repositorio en una sola transacción de base de datos
/// (ej. el cambio de un ticket y las notificaciones que genera).
/// </summary>
public interface IUnidadDeTrabajo
{
    /// <summary>Confirma la transacción si <paramref name="operacion"/> termina sin excepción.</summary>
    Task<T> EjecutarAsync<T>(Func<Task<T>> operacion);
}
