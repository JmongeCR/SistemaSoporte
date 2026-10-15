using Microsoft.AspNetCore.Mvc;
using Soporte.Abstracciones.DTOs;
using Soporte.Abstracciones.Enumeraciones;

namespace Soporte.API.Controllers;

/// <summary>Traduce <see cref="ResultadoOperacion"/> a respuestas HTTP con formato ProblemDetails (RFC 7807).</summary>
public abstract class SoporteControllerBase : ControllerBase
{
    /// <summary>5 archivos de 10 MB más margen para los demás campos del formulario.</summary>
    protected const long LimiteSubidaBytes = 55 * 1024 * 1024;

    /// <summary>Convierte los archivos del formulario al DTO de Abstracciones (Flujo y Reglas no dependen de ASP.NET).</summary>
    protected static List<ArchivoEntranteDto> ConvertirArchivos(IEnumerable<IFormFile>? archivos)
    {
        return (archivos ?? Enumerable.Empty<IFormFile>())
            .Select(a => new ArchivoEntranteDto
            {
                NombreOriginal = Path.GetFileName(a.FileName),
                TipoContenido = a.ContentType,
                TamanoBytes = a.Length,
                Contenido = a.OpenReadStream()
            })
            .ToList();
    }

    protected static void LiberarArchivos(IEnumerable<ArchivoEntranteDto> archivos)
    {
        foreach (var archivo in archivos)
            archivo.Contenido.Dispose();
    }

    protected ObjectResult RespuestaError(ResultadoOperacion resultado)
    {
        var codigo = resultado.TipoError switch
        {
            TipoError.NoEncontrado => StatusCodes.Status404NotFound,
            TipoError.SinPermiso => StatusCodes.Status403Forbidden,
            TipoError.Conflicto => StatusCodes.Status409Conflict,
            TipoError.NoAutenticado => StatusCodes.Status401Unauthorized,
            _ => StatusCodes.Status400BadRequest
        };

        var problema = ProblemDetailsFactory.CreateProblemDetails(HttpContext, codigo, title: resultado.Mensaje);
        if (resultado.Errores.Count > 0)
            problema.Extensions["errores"] = resultado.Errores;

        return new ObjectResult(problema) { StatusCode = codigo };
    }

    protected ObjectResult NoAutenticado()
    {
        return Problem(title: "Debe iniciar sesión para usar este recurso.", statusCode: StatusCodes.Status401Unauthorized);
    }
}
