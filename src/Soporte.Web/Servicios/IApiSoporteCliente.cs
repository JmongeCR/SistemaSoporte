using Soporte.Abstracciones.DTOs;

namespace Soporte.Web.Servicios;

/// <summary>Acceso HTTP a Soporte.API. Traduce errores de red y ProblemDetails a <see cref="ResultadoOperacion{T}"/>.</summary>
public interface IApiSoporteCliente
{
    Task<ResultadoOperacion<T>> GetAsync<T>(string ruta);
    Task<ResultadoOperacion<T>> PostMultipartAsync<T>(string ruta, MultipartFormDataContent contenido);
    Task<ResultadoOperacion<T>> PostJsonAsync<T>(string ruta, object cuerpo);
    Task<ResultadoOperacion<T>> PutJsonAsync<T>(string ruta, object cuerpo);

    /// <summary>POST sin cuerpo para acciones cuya respuesta no trae datos (ej. 204).</summary>
    Task<ResultadoOperacion> PostSinCuerpoAsync(string ruta);

    /// <summary>Descarga un archivo (la autorización la decide el API con el token del usuario).</summary>
    Task<ResultadoOperacion<ArchivoDescargaDto>> DescargarAsync(string ruta);
}
