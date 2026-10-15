using Microsoft.EntityFrameworkCore;
using Soporte.Abstracciones.Interfaces.DA;
using Soporte.DA.Contexto;

namespace Soporte.DA.Repositorios;

/// <summary>
/// Transacción sobre el DbContext compartido (scoped) por los repositorios de la petición.
/// Usa la estrategia de ejecución de EF para convivir con EnableRetryOnFailure.
/// </summary>
public class UnidadDeTrabajo : IUnidadDeTrabajo
{
    private readonly SoporteDbContext _contexto;

    public UnidadDeTrabajo(SoporteDbContext contexto)
    {
        _contexto = contexto;
    }

    public async Task<T> EjecutarAsync<T>(Func<Task<T>> operacion)
    {
        // Si ya hay una transacción abierta (llamada anidada), se reutiliza.
        if (_contexto.Database.CurrentTransaction is not null)
            return await operacion();

        var estrategia = _contexto.Database.CreateExecutionStrategy();
        return await estrategia.ExecuteAsync(async () =>
        {
            await using var transaccion = await _contexto.Database.BeginTransactionAsync();
            var resultado = await operacion();
            await transaccion.CommitAsync();
            return resultado;
        });
    }
}
