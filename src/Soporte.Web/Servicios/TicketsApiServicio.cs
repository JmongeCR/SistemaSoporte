using System.Net.Http.Headers;
using Microsoft.AspNetCore.WebUtilities;
using Soporte.Abstracciones.DTOs;

namespace Soporte.Web.Servicios;

public class TicketsApiServicio : ITicketsApiServicio
{
    private const string Ruta = "api/tickets";

    private readonly IApiSoporteCliente _api;

    public TicketsApiServicio(IApiSoporteCliente api)
    {
        _api = api;
    }

    public async Task<ResultadoOperacion<ResultadoPaginado<TicketResumenDto>>> ListarAsync(FiltroTicketsDto filtro)
    {
        var parametros = new Dictionary<string, string?>
        {
            [nameof(FiltroTicketsDto.EstadoId)] = filtro.EstadoId?.ToString(),
            [nameof(FiltroTicketsDto.PrioridadId)] = filtro.PrioridadId?.ToString(),
            [nameof(FiltroTicketsDto.AgenteId)] = filtro.AgenteId?.ToString(),
            [nameof(FiltroTicketsDto.SoloSinAsignar)] = filtro.SoloSinAsignar ? "true" : null,
            [nameof(FiltroTicketsDto.RequiereAtencion)] = filtro.RequiereAtencion ? "true" : null,
            [nameof(FiltroTicketsDto.OrdenarPorActividad)] = filtro.OrdenarPorActividad ? "true" : null,
            [nameof(FiltroTicketsDto.Texto)] = string.IsNullOrWhiteSpace(filtro.Texto) ? null : filtro.Texto.Trim(),
            [nameof(FiltroTicketsDto.Pagina)] = filtro.Pagina.ToString(),
            [nameof(FiltroTicketsDto.TamanoPagina)] = filtro.TamanoPagina.ToString()
        };

        var ruta = QueryHelpers.AddQueryString(Ruta, parametros.Where(p => p.Value is not null));
        return await _api.GetAsync<ResultadoPaginado<TicketResumenDto>>(ruta);
    }

    public async Task<ResultadoOperacion<TicketDetalleDto>> CrearAsync(CrearTicketDto ticket, IReadOnlyList<IFormFile> adjuntos)
    {
        using var contenido = new MultipartFormDataContent
        {
            { new StringContent(ticket.Asunto), nameof(CrearTicketDto.Asunto) },
            { new StringContent(ticket.Descripcion), nameof(CrearTicketDto.Descripcion) },
            { new StringContent(ticket.CategoriaId.ToString()), nameof(CrearTicketDto.CategoriaId) },
            { new StringContent(ticket.PrioridadId.ToString()), nameof(CrearTicketDto.PrioridadId) }
        };
        if (ticket.ClienteId.HasValue)
            contenido.Add(new StringContent(ticket.ClienteId.Value.ToString()), nameof(CrearTicketDto.ClienteId));

        AgregarArchivos(contenido, adjuntos);

        return await _api.PostMultipartAsync<TicketDetalleDto>(Ruta, contenido);
    }

    public async Task<ResultadoOperacion<TicketDetalleDto>> ObtenerAsync(int id) =>
        await _api.GetAsync<TicketDetalleDto>($"{Ruta}/{id}");

    public async Task<ResultadoOperacion<TicketDetalleDto>> ActualizarAsync(int id, ActualizarTicketDto cambios) =>
        await _api.PutJsonAsync<TicketDetalleDto>($"{Ruta}/{id}", cambios);

    public async Task<ResultadoOperacion> TomarAsync(int id) =>
        await _api.PostSinCuerpoAsync($"{Ruta}/{id}/tomar");

    public async Task<ResultadoOperacion<ComentarioDto>> ComentarAsync(int id, CrearComentarioDto comentario, IReadOnlyList<IFormFile> adjuntos)
    {
        using var contenido = new MultipartFormDataContent
        {
            { new StringContent(comentario.Texto), nameof(CrearComentarioDto.Texto) },
            { new StringContent(comentario.EsNotaInterna.ToString()), nameof(CrearComentarioDto.EsNotaInterna) },
            { new StringContent(comentario.SolicitaInformacion.ToString()), nameof(CrearComentarioDto.SolicitaInformacion) }
        };
        AgregarArchivos(contenido, adjuntos);

        return await _api.PostMultipartAsync<ComentarioDto>($"{Ruta}/{id}/comentarios", contenido);
    }

    public async Task<ResultadoOperacion<ArchivoDescargaDto>> DescargarAdjuntoAsync(int adjuntoId) =>
        await _api.DescargarAsync($"api/adjuntos/{adjuntoId}");

    private static void AgregarArchivos(MultipartFormDataContent contenido, IReadOnlyList<IFormFile> adjuntos)
    {
        foreach (var adjunto in adjuntos)
        {
            var parte = new StreamContent(adjunto.OpenReadStream());
            parte.Headers.ContentType = MediaTypeHeaderValue.TryParse(adjunto.ContentType, out var tipo)
                ? tipo
                : new MediaTypeHeaderValue("application/octet-stream");
            contenido.Add(parte, "adjuntos", adjunto.FileName);
        }
    }
}
