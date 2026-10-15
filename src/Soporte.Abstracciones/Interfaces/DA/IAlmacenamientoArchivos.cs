namespace Soporte.Abstracciones.Interfaces.DA;

/// <summary>Persistencia física de adjuntos (disco local o blob). La ruta base viene de appsettings.json.</summary>
public interface IAlmacenamientoArchivos
{
    /// <returns>Ruta relativa con la que se recupera el archivo.</returns>
    Task<string> GuardarAsync(Stream contenido, string nombreOriginal);
    Task<Stream> AbrirAsync(string ruta);
}
