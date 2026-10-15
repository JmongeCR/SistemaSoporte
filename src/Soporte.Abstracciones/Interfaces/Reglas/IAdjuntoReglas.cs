using Soporte.Abstracciones.DTOs;

namespace Soporte.Abstracciones.Interfaces.Reglas;

public interface IAdjuntoReglas
{
    /// <summary>HU-11: solo PDF, PNG o JPG, menor a 10 MB y con contenido acorde a la extensión.</summary>
    ResultadoOperacion ValidarArchivo(ArchivoEntranteDto archivo);

    /// <summary>Valida la cantidad por envío y cada archivo; devuelve el primer error encontrado.</summary>
    ResultadoOperacion ValidarArchivos(IReadOnlyList<ArchivoEntranteDto> archivos);
}
