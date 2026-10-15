using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Soporte.Abstracciones.DTOs;
using Soporte.Abstracciones.Enumeraciones;
using Soporte.Web.Servicios;

namespace Soporte.Web.Pages.Tickets;

public class IndexModel : PageModel
{
    private readonly ITicketsApiServicio _ticketsApi;
    private readonly ICatalogosApiServicio _catalogosApi;
    private readonly IUsuarioSesion _sesion;

    public IndexModel(ITicketsApiServicio ticketsApi, ICatalogosApiServicio catalogosApi, IUsuarioSesion sesion)
    {
        _ticketsApi = ticketsApi;
        _catalogosApi = catalogosApi;
        _sesion = sesion;
    }

    /// <summary>HU-01 esc. 1: el Cliente ve "Mis tickets" (el API ya limita la lista a los suyos).</summary>
    public bool EsCliente => _sesion.Rol == RolTipo.Cliente;
    public string Titulo => EsCliente ? "Mis tickets" : "Tickets";

    [BindProperty(SupportsGet = true)]
    public FiltroTicketsDto Filtro { get; set; } = new();

    public ResultadoPaginado<TicketResumenDto>? Tickets { get; private set; }
    public int? TotalAbiertos { get; private set; }
    public IReadOnlyList<CatalogoDto> Estados { get; private set; } = Array.Empty<CatalogoDto>();
    public IReadOnlyList<SelectListItem> Prioridades { get; private set; } = Array.Empty<SelectListItem>();
    public string? MensajeError { get; private set; }

    public bool HayFiltros =>
        Filtro.EstadoId.HasValue || Filtro.PrioridadId.HasValue || Filtro.SoloSinAsignar || !string.IsNullOrWhiteSpace(Filtro.Texto);

    public async Task OnGetAsync()
    {
        // Parámetros manipulados en la URL (ej. Pagina=0): se vuelve al filtro por defecto.
        if (!ModelState.IsValid)
        {
            ModelState.Clear();
            Filtro = new FiltroTicketsDto();
        }

        var tareaTickets = _ticketsApi.ListarAsync(Filtro);
        var tareaAbiertos = _ticketsApi.ListarAsync(new FiltroTicketsDto { EstadoId = (int)EstadoTicketTipo.Abierto, TamanoPagina = 1 });
        var tareaEstados = _catalogosApi.ListarEstadosAsync();
        var tareaPrioridades = _catalogosApi.ListarPrioridadesAsync();
        await Task.WhenAll(tareaTickets, tareaAbiertos, tareaEstados, tareaPrioridades);

        var tickets = tareaTickets.Result;
        if (tickets.Exito)
            Tickets = tickets.Datos;
        else
            MensajeError = tickets.Mensaje;

        TotalAbiertos = tareaAbiertos.Result.Datos?.Total;
        Estados = tareaEstados.Result.Datos ?? Array.Empty<CatalogoDto>();
        Prioridades = (tareaPrioridades.Result.Datos ?? Array.Empty<CatalogoDto>())
            .Select(p => new SelectListItem(p.Nombre, p.Id.ToString(), p.Id == Filtro.PrioridadId))
            .ToList();
    }

    /// <summary>Parámetros de ruta para los enlaces de paginación, conservando los filtros actuales.</summary>
    public Dictionary<string, string> RutaPagina(int pagina)
    {
        var ruta = new Dictionary<string, string> { [$"{nameof(Filtro)}.{nameof(FiltroTicketsDto.Pagina)}"] = pagina.ToString() };
        void Agregar(string nombre, object? valor)
        {
            if (valor is not null && valor.ToString() is { Length: > 0 } texto)
                ruta[$"{nameof(Filtro)}.{nombre}"] = texto;
        }

        Agregar(nameof(FiltroTicketsDto.EstadoId), Filtro.EstadoId);
        Agregar(nameof(FiltroTicketsDto.PrioridadId), Filtro.PrioridadId);
        Agregar(nameof(FiltroTicketsDto.Texto), Filtro.Texto);
        Agregar(nameof(FiltroTicketsDto.SoloSinAsignar), Filtro.SoloSinAsignar ? "true" : null);
        Agregar(nameof(FiltroTicketsDto.TamanoPagina), Filtro.TamanoPagina != new FiltroTicketsDto().TamanoPagina ? Filtro.TamanoPagina : null);
        return ruta;
    }
}
