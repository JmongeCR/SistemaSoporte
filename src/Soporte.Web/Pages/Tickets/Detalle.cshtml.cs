using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Soporte.Abstracciones.DTOs;
using Soporte.Abstracciones.Enumeraciones;
using Soporte.Web.Presentacion;
using Soporte.Web.Servicios;

namespace Soporte.Web.Pages.Tickets;

/// <summary>
/// Detalle del ticket (diseño 04): datos, conversación y avances (HU-06 a HU-11).
/// Los permisos los decide el API; aquí solo se ocultan opciones que el usuario no puede usar.
/// </summary>
[RequestSizeLimit(LimiteSubidaBytes)]
[RequestFormLimits(MultipartBodyLengthLimit = LimiteSubidaBytes)]
public class DetalleModel : PageModel
{
    private const long LimiteSubidaBytes = 55 * 1024 * 1024;

    private readonly ITicketsApiServicio _ticketsApi;
    private readonly ICatalogosApiServicio _catalogosApi;
    private readonly IUsuariosApiServicio _usuariosApi;
    private readonly IUsuarioSesion _sesion;
    private readonly IZonaHoraria _zonaHoraria;

    public DetalleModel(
        ITicketsApiServicio ticketsApi,
        ICatalogosApiServicio catalogosApi,
        IUsuariosApiServicio usuariosApi,
        IUsuarioSesion sesion,
        IZonaHoraria zonaHoraria)
    {
        _ticketsApi = ticketsApi;
        _catalogosApi = catalogosApi;
        _usuariosApi = usuariosApi;
        _sesion = sesion;
        _zonaHoraria = zonaHoraria;
    }

    [BindProperty(SupportsGet = true)]
    public int Id { get; set; }

    public TicketDetalleDto? Ticket { get; private set; }
    public IReadOnlyList<ItemActividad> Actividad { get; private set; } = Array.Empty<ItemActividad>();
    public string? MensajeError { get; private set; }

    /// <summary>Formularios: se conservan si el envío falla, para no perder lo escrito.</summary>
    public CrearComentarioDto Respuesta { get; private set; } = new();
    public string TipoRespuesta { get; private set; } = TiposRespuesta.Publica;
    public ActualizarTicketDto Cambios { get; private set; } = new();

    public IReadOnlyList<SelectListItem> Estados { get; private set; } = Array.Empty<SelectListItem>();
    public IReadOnlyList<SelectListItem> Prioridades { get; private set; } = Array.Empty<SelectListItem>();
    public IReadOnlyList<SelectListItem> Categorias { get; private set; } = Array.Empty<SelectListItem>();
    public IReadOnlyList<SelectListItem> Agentes { get; private set; } = Array.Empty<SelectListItem>();

    public bool EsCliente => _sesion.Rol == RolTipo.Cliente;
    public bool EsAdministrador => _sesion.Rol == RolTipo.Administrador;
    public bool EsEquipo => !EsCliente;
    public bool EstaCerrado => Ticket?.EstadoId == (int)EstadoTicketTipo.Cerrado;

    /// <summary>Administrador siempre; Agente solo si el ticket está asignado a él.</summary>
    public bool PuedeGestionar => EsAdministrador || (_sesion.Rol == RolTipo.Agente && Ticket?.AgenteAsignadoId == _sesion.UsuarioId);

    public bool PuedeTomar => _sesion.Rol == RolTipo.Agente && Ticket is { AgenteAsignadoId: null }
                              && Ticket.EstadoId is (int)EstadoTicketTipo.Abierto or (int)EstadoTicketTipo.EnProgreso;

    public bool PuedeReabrir => EsCliente && Ticket?.EstadoId == (int)EstadoTicketTipo.Resuelto;

    /// <summary>HU-08 esc. 3: con el ticket cerrado solo el equipo deja notas internas.</summary>
    public bool PuedeResponder => !EstaCerrado || EsEquipo;

    public string FechaLocal(DateTime fechaUtc) => _zonaHoraria.ALocal(fechaUtc).ToString("dd/MM/yyyy HH:mm");

    public async Task<IActionResult> OnGetAsync()
    {
        await CargarAsync();
        return Pagina();
    }

    /// <summary>Descarga un adjunto con el token del usuario: el API decide si tiene acceso.</summary>
    public async Task<IActionResult> OnGetAdjuntoAsync(int adjuntoId)
    {
        var resultado = await _ticketsApi.DescargarAdjuntoAsync(adjuntoId);
        if (!resultado.Exito)
            return NotFound();

        var archivo = resultado.Datos!;
        return File(archivo.Contenido, archivo.TipoContenido, archivo.NombreOriginal);
    }

    public async Task<IActionResult> OnPostComentarAsync(
        [Bind(Prefix = nameof(Respuesta))] CrearComentarioDto respuesta, string? tipoRespuesta, List<IFormFile> adjuntos)
    {
        Respuesta = respuesta;
        TipoRespuesta = tipoRespuesta ?? TiposRespuesta.Publica;
        respuesta.EsNotaInterna = EsEquipo && TipoRespuesta == TiposRespuesta.Interna;
        respuesta.SolicitaInformacion = EsEquipo && TipoRespuesta == TiposRespuesta.Solicitud;

        if (!ModelState.IsValid)
            return await RecargarConErrorAsync(null);

        var resultado = await _ticketsApi.ComentarAsync(Id, respuesta, adjuntos);
        if (!resultado.Exito)
            return await RecargarConErrorAsync(resultado.Mensaje, resultado.Errores, adjuntos.Count > 0);

        // El API devuelve el comentario creado, no un texto: el mensaje se arma aquí.
        TempData["MensajeExito"] = respuesta.EsNotaInterna
            ? "Nota interna guardada. El cliente no la ve."
            : respuesta.SolicitaInformacion
                ? "Solicitud enviada. Le avisamos al cliente."
                : "Respuesta enviada.";
        return RedirectToPage(null, null, new { id = Id }, "actividad");
    }

    public async Task<IActionResult> OnPostActualizarAsync([Bind(Prefix = nameof(Cambios))] ActualizarTicketDto cambios)
    {
        Cambios = cambios;
        if (!ModelState.IsValid)
            return await RecargarConErrorAsync(null);

        var antes = await _ticketsApi.ObtenerAsync(Id);
        var resultado = await _ticketsApi.ActualizarAsync(Id, cambios);
        if (!resultado.Exito)
            return await RecargarConErrorAsync(resultado.Mensaje, resultado.Errores);

        // El API no guarda ni notifica si nada cambió; la Web lo informa en lugar de decir "guardado".
        var sinCambios = antes.Datos is { } previo
                         && string.IsNullOrWhiteSpace(cambios.MensajePublico)
                         && previo.EstadoId == cambios.EstadoId
                         && previo.PrioridadId == cambios.PrioridadId
                         && previo.CategoriaId == cambios.CategoriaId
                         && previo.AgenteAsignadoId == cambios.AgenteAsignadoId;
        TempData["MensajeExito"] = sinCambios ? "No había cambios para guardar." : "Cambios guardados.";
        return RedirectToPage(null, null, new { id = Id }, null);
    }

    public async Task<IActionResult> OnPostTomarAsync()
    {
        var resultado = await _ticketsApi.TomarAsync(Id);
        TempData[resultado.Exito ? "MensajeExito" : "MensajeError"] = resultado.Exito ? "Te asignaste el ticket." : resultado.Mensaje;
        return RedirectToPage(null, null, new { id = Id }, null);
    }

    /// <summary>HU-08 esc. 2: el cliente reabre un ticket resuelto, con un motivo opcional.</summary>
    public async Task<IActionResult> OnPostReabrirAsync(string rowVersion, string? motivo)
    {
        var actual = await _ticketsApi.ObtenerAsync(Id);
        if (!actual.Exito)
            return await RecargarConErrorAsync(actual.Mensaje);

        var ticket = actual.Datos!;
        var resultado = await _ticketsApi.ActualizarAsync(Id, new ActualizarTicketDto
        {
            EstadoId = (int)EstadoTicketTipo.Abierto,
            PrioridadId = ticket.PrioridadId,
            CategoriaId = ticket.CategoriaId,
            AgenteAsignadoId = ticket.AgenteAsignadoId,
            RowVersion = rowVersion,
            MensajePublico = motivo
        });

        if (!resultado.Exito)
            return await RecargarConErrorAsync(resultado.Mensaje, resultado.Errores);

        TempData["MensajeExito"] = "Reabriste el ticket. El equipo va a revisarlo de nuevo.";
        return RedirectToPage(null, null, new { id = Id }, null);
    }

    private async Task<IActionResult> RecargarConErrorAsync(string? mensaje, IReadOnlyList<string>? errores = null, bool habiaAdjuntos = false)
    {
        var textoCambios = Cambios;
        await CargarAsync();
        if (textoCambios.EstadoId != 0)
            Cambios = textoCambios;

        var detalle = errores is { Count: > 0 } ? " " + string.Join(" ", errores) : string.Empty;
        var aviso = habiaAdjuntos ? " Volvé a seleccionar los adjuntos." : string.Empty;
        MensajeError = mensaje is null ? MensajeError : mensaje + detalle + aviso;
        return Pagina();
    }

    private IActionResult Pagina()
    {
        if (Ticket is null)
            Response.StatusCode = StatusCodes.Status404NotFound;
        return Page();
    }

    private async Task CargarAsync()
    {
        var resultado = await _ticketsApi.ObtenerAsync(Id);
        if (!resultado.Exito)
        {
            MensajeError = resultado.Mensaje;
            return;
        }

        Ticket = resultado.Datos!;
        Actividad = ItemActividad.Combinar(Ticket);
        Cambios = new ActualizarTicketDto
        {
            EstadoId = Ticket.EstadoId,
            PrioridadId = Ticket.PrioridadId,
            CategoriaId = Ticket.CategoriaId,
            AgenteAsignadoId = Ticket.AgenteAsignadoId,
            RowVersion = Ticket.RowVersion
        };

        if (!PuedeGestionar)
            return;

        var estados = _catalogosApi.ListarEstadosAsync();
        var prioridades = _catalogosApi.ListarPrioridadesAsync();
        var categorias = _catalogosApi.ListarCategoriasAsync();
        var agentes = EsAdministrador
            ? _usuariosApi.ListarAgentesAsync()
            : Task.FromResult(ResultadoOperacion<IReadOnlyList<UsuarioDto>>.Ok(Array.Empty<UsuarioDto>()));
        await Task.WhenAll(estados, prioridades, categorias, agentes);

        Estados = Opciones(estados.Result.Datos);
        Prioridades = Opciones(prioridades.Result.Datos);
        Categorias = Opciones(categorias.Result.Datos);
        Agentes = (agentes.Result.Datos ?? Array.Empty<UsuarioDto>())
            .Where(a => a.Activo)
            .Select(a => new SelectListItem(a.Nombre, a.Id.ToString()))
            .ToList();
    }

    private static IReadOnlyList<SelectListItem> Opciones(IReadOnlyList<CatalogoDto>? catalogo) =>
        (catalogo ?? Array.Empty<CatalogoDto>()).Select(c => new SelectListItem(c.Nombre, c.Id.ToString())).ToList();

    public static class TiposRespuesta
    {
        public const string Publica = "publica";
        public const string Interna = "interna";
        public const string Solicitud = "solicitud";
    }
}
