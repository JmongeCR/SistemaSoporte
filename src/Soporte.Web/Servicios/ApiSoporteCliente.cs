using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Soporte.Abstracciones.DTOs;

namespace Soporte.Web.Servicios;

public class ApiSoporteCliente : IApiSoporteCliente
{
    /// <summary>Nombre del cliente registrado con AddHttpClient en Program.cs.</summary>
    public const string NombreClienteHttp = "SoporteApi";

    private const string SesionInvalida = "Tu sesión ya no es válida (por ejemplo, cambió tu rol). Iniciá sesión de nuevo.";

    private readonly IHttpClientFactory _fabricaHttp;
    private readonly ILogger<ApiSoporteCliente> _logger;
    private readonly IUsuarioSesion _sesion;
    private readonly IGestorSesion _gestorSesion;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ApiSoporteCliente(
        IHttpClientFactory fabricaHttp,
        ILogger<ApiSoporteCliente> logger,
        IUsuarioSesion sesion,
        IGestorSesion gestorSesion,
        IHttpContextAccessor httpContextAccessor)
    {
        _fabricaHttp = fabricaHttp;
        _logger = logger;
        _sesion = sesion;
        _gestorSesion = gestorSesion;
        _httpContextAccessor = httpContextAccessor;
    }

    public Task<ResultadoOperacion<T>> GetAsync<T>(string ruta) =>
        EnviarAsync<T>(ruta, cliente => cliente.GetAsync(ruta));

    public Task<ResultadoOperacion<T>> PostMultipartAsync<T>(string ruta, MultipartFormDataContent contenido) =>
        EnviarAsync<T>(ruta, cliente => cliente.PostAsync(ruta, contenido));

    public Task<ResultadoOperacion<T>> PostJsonAsync<T>(string ruta, object cuerpo) =>
        EnviarAsync<T>(ruta, cliente => cliente.PostAsJsonAsync(ruta, cuerpo));

    public Task<ResultadoOperacion<T>> PutJsonAsync<T>(string ruta, object cuerpo) =>
        EnviarAsync<T>(ruta, cliente => cliente.PutAsJsonAsync(ruta, cuerpo));

    public async Task<ResultadoOperacion> PostSinCuerpoAsync(string ruta) =>
        await EnviarAsync(ruta, cliente => cliente.PostAsync(ruta, content: null), _ => Task.FromResult<MensajeDto?>(new MensajeDto()));

    public Task<ResultadoOperacion<ArchivoDescargaDto>> DescargarAsync(string ruta) =>
        EnviarAsync(ruta, cliente => cliente.GetAsync(ruta), async respuesta =>
        {
            // Se copia a memoria porque la respuesta HTTP se libera al terminar (máximo 10 MB por adjunto).
            var memoria = new MemoryStream();
            await respuesta.Content.CopyToAsync(memoria);
            memoria.Position = 0;
            return (ArchivoDescargaDto?)new ArchivoDescargaDto
            {
                NombreOriginal = respuesta.Content.Headers.ContentDisposition?.FileNameStar
                                 ?? respuesta.Content.Headers.ContentDisposition?.FileName?.Trim('"')
                                 ?? "adjunto",
                TipoContenido = respuesta.Content.Headers.ContentType?.MediaType ?? "application/octet-stream",
                Contenido = memoria
            };
        });

    private Task<ResultadoOperacion<T>> EnviarAsync<T>(string ruta, Func<HttpClient, Task<HttpResponseMessage>> enviar) =>
        EnviarAsync(ruta, enviar, respuesta => respuesta.Content.ReadFromJsonAsync<T>());

    private async Task<ResultadoOperacion<T>> EnviarAsync<T>(
        string ruta, Func<HttpClient, Task<HttpResponseMessage>> enviar, Func<HttpResponseMessage, Task<T?>> leer)
    {
        var cliente = _fabricaHttp.CreateClient(NombreClienteHttp);

        try
        {
            using var respuesta = await enviar(cliente);

            // Con sesión iniciada, un 401 significa que el API ya no acepta el token (rol cambiado,
            // cuenta desactivada o token vencido): se cierra la sesión de la Web también.
            if (respuesta.StatusCode == HttpStatusCode.Unauthorized && _sesion.EstaAutenticado)
            {
                await CerrarSesionAsync();
                return ResultadoOperacion<T>.NoAutenticado(SesionInvalida);
            }

            if (!respuesta.IsSuccessStatusCode)
                return await CrearErrorAsync<T>(respuesta);

            var datos = await leer(respuesta);
            return datos is null
                ? ResultadoOperacion<T>.Fallo("El API devolvió una respuesta vacía.")
                : ResultadoOperacion<T>.Ok(datos);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "No se pudo conectar con Soporte.API ({Ruta}).", ruta);
            return ResultadoOperacion<T>.Fallo("No se pudo conectar con el API de soporte. Verifique que Soporte.API esté en ejecución.");
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(ex, "Soporte.API no respondió a tiempo ({Ruta}).", ruta);
            return ResultadoOperacion<T>.Fallo("El API de soporte tardó demasiado en responder. Intente de nuevo.");
        }
    }

    private async Task CerrarSesionAsync()
    {
        // Si la respuesta ya empezó a enviarse (ej. desde un componente de la vista) no se puede
        // borrar la cookie; la próxima petición la rechazará igual al validar el token.
        if (_httpContextAccessor.HttpContext is { Response.HasStarted: false })
            await _gestorSesion.CerrarAsync();
    }

    private static async Task<ResultadoOperacion<T>> CrearErrorAsync<T>(HttpResponseMessage respuesta)
    {
        string? titulo = null;
        var errores = Array.Empty<string>();
        try
        {
            // ValidationProblemDetails también lee los ProblemDetails simples (sin "errors").
            var problema = await respuesta.Content.ReadFromJsonAsync<ValidationProblemDetails>();
            titulo = problema?.Title;
            errores = problema?.Errors.SelectMany(e => e.Value).Distinct().ToArray() ?? Array.Empty<string>();
        }
        catch (Exception ex) when (ex is JsonException or NotSupportedException)
        {
            // La respuesta no es ProblemDetails: se usa el mensaje genérico.
        }

        var mensaje = titulo ?? $"El API respondió con el código {(int)respuesta.StatusCode}.";

        return respuesta.StatusCode switch
        {
            HttpStatusCode.NotFound => ResultadoOperacion<T>.NoEncontrado(mensaje),
            HttpStatusCode.Forbidden => ResultadoOperacion<T>.SinPermiso(mensaje),
            HttpStatusCode.Conflict => ResultadoOperacion<T>.Conflicto(mensaje),
            HttpStatusCode.Unauthorized => ResultadoOperacion<T>.NoAutenticado(mensaje),
            _ => ResultadoOperacion<T>.Fallo(mensaje, errores)
        };
    }
}
