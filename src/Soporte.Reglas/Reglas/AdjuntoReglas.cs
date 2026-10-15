using Soporte.Abstracciones.DTOs;
using Soporte.Abstracciones.Interfaces.Reglas;
using Soporte.Abstracciones.Modelos;

namespace Soporte.Reglas.Reglas;

public class AdjuntoReglas : IAdjuntoReglas
{
    public const int MaximoPorEnvio = 5;
    private const string FormatosValidos = "Formatos válidos: PDF, PNG o JPG.";

    /// <summary>Extensión → tipos MIME aceptados y firma ("magic bytes") con la que empieza el archivo.</summary>
    private static readonly Dictionary<string, (string[] Tipos, byte[] Firma)> Formatos = new(StringComparer.OrdinalIgnoreCase)
    {
        [".pdf"] = (new[] { "application/pdf" }, "%PDF"u8.ToArray()),
        [".png"] = (new[] { "image/png" }, new byte[] { 0x89, 0x50, 0x4E, 0x47 }),
        [".jpg"] = (new[] { "image/jpeg" }, new byte[] { 0xFF, 0xD8, 0xFF }),
        [".jpeg"] = (new[] { "image/jpeg" }, new byte[] { 0xFF, 0xD8, 0xFF })
    };

    public ResultadoOperacion ValidarArchivos(IReadOnlyList<ArchivoEntranteDto> archivos)
    {
        if (archivos.Count > MaximoPorEnvio)
            return ResultadoOperacion.Fallo($"Puede adjuntar hasta {MaximoPorEnvio} archivos por envío.");

        foreach (var archivo in archivos)
        {
            var resultado = ValidarArchivo(archivo);
            if (!resultado.Exito)
                return resultado;
        }

        return ResultadoOperacion.Ok();
    }

    public ResultadoOperacion ValidarArchivo(ArchivoEntranteDto archivo)
    {
        var nombre = archivo.NombreOriginal;

        if (!Formatos.TryGetValue(Path.GetExtension(nombre), out var formato)
            || !formato.Tipos.Contains(archivo.TipoContenido, StringComparer.OrdinalIgnoreCase))
            return ResultadoOperacion.Fallo($"\"{nombre}\" no tiene un formato permitido. {FormatosValidos}");

        if (archivo.TamanoBytes <= 0)
            return ResultadoOperacion.Fallo($"\"{nombre}\" está vacío.");

        if (archivo.TamanoBytes > Adjunto.TamanoMaximoBytes)
            return ResultadoOperacion.Fallo($"\"{nombre}\" supera el límite de 10 MB.");

        if (!EmpiezaCon(archivo.Contenido, formato.Firma))
            return ResultadoOperacion.Fallo($"El contenido de \"{nombre}\" no corresponde a su extensión. {FormatosValidos}");

        return ResultadoOperacion.Ok();
    }

    /// <summary>Lee los primeros bytes y vuelve a la posición original para no afectar el guardado.</summary>
    private static bool EmpiezaCon(Stream contenido, byte[] firma)
    {
        if (!contenido.CanSeek)
            return false;

        var posicion = contenido.Position;
        try
        {
            contenido.Position = 0;
            Span<byte> inicio = stackalloc byte[firma.Length];
            return contenido.ReadAtLeast(inicio, firma.Length, throwOnEndOfStream: false) == firma.Length
                   && inicio.SequenceEqual(firma);
        }
        finally
        {
            contenido.Position = posicion;
        }
    }
}
