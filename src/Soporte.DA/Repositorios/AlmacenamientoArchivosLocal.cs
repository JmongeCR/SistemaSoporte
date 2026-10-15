using Soporte.Abstracciones.Interfaces.DA;

namespace Soporte.DA.Repositorios;

/// <summary>
/// Guarda los adjuntos en disco bajo una carpeta base (configurada en appsettings.json).
/// Los nombres se generan (GUID): nunca se usa el nombre enviado por el usuario como ruta.
/// </summary>
public class AlmacenamientoArchivosLocal : IAlmacenamientoArchivos
{
    private const int TamanoBuffer = 81920;

    private readonly string _rutaBase;

    public AlmacenamientoArchivosLocal(string rutaBase)
    {
        _rutaBase = Path.GetFullPath(rutaBase);
    }

    public async Task<string> GuardarAsync(Stream contenido, string nombreOriginal)
    {
        var ahora = DateTime.UtcNow;
        var relativa = Path.Combine(
            ahora.ToString("yyyy"),
            ahora.ToString("MM"),
            $"{Guid.NewGuid():N}{Path.GetExtension(nombreOriginal).ToLowerInvariant()}");
        var completa = Path.Combine(_rutaBase, relativa);

        Directory.CreateDirectory(Path.GetDirectoryName(completa)!);

        if (contenido.CanSeek)
            contenido.Position = 0;

        await using var destino = new FileStream(completa, FileMode.CreateNew, FileAccess.Write, FileShare.None, TamanoBuffer, useAsync: true);
        await contenido.CopyToAsync(destino);

        return relativa.Replace(Path.DirectorySeparatorChar, '/');
    }

    public Task<Stream> AbrirAsync(string ruta)
    {
        var completa = Path.GetFullPath(Path.Combine(_rutaBase, ruta));

        // Defensa ante rutas manipuladas ("../"): el archivo debe quedar dentro de la carpeta base.
        if (!completa.StartsWith(_rutaBase + Path.DirectorySeparatorChar, StringComparison.Ordinal))
            throw new UnauthorizedAccessException("La ruta del adjunto no es válida.");

        Stream archivo = new FileStream(completa, FileMode.Open, FileAccess.Read, FileShare.Read, TamanoBuffer, useAsync: true);
        return Task.FromResult(archivo);
    }
}
