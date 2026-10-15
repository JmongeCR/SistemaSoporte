using Soporte.Abstracciones.Modelos;

namespace Soporte.Abstracciones.Interfaces.DA;

public interface IAdjuntoRepository
{
    Task<Adjunto?> ObtenerPorIdAsync(int id);
    Task<int> CrearAsync(Adjunto adjunto);
}
