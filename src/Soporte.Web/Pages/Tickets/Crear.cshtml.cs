using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Caching.Memory;
using Soporte.Abstracciones.DTOs;
using Soporte.Abstracciones.Enumeraciones;
using Soporte.Web.Servicios;

namespace Soporte.Web.Pages.Tickets;

/// <summary>HU-04: crear ticket (diseño "05 - Crear Ticket").</summary>
[RequestSizeLimit(LimiteSubidaBytes)]
[RequestFormLimits(MultipartBodyLengthLimit = LimiteSubidaBytes)]
public class CrearModel : PageModel
{
    private const long LimiteSubidaBytes = 55 * 1024 * 1024;

    private readonly ITicketsApiServicio _ticketsApi;
    private readonly ICatalogosApiServicio _catalogosApi;
    private readonly IUsuariosApiServicio _usuariosApi;
    private readonly IUsuarioSesion _sesion;
    private readonly IMemoryCache _envios;

    public CrearModel(
        ITicketsApiServicio ticketsApi,
        ICatalogosApiServicio catalogosApi,
        IUsuariosApiServicio usuariosApi,
        IUsuarioSesion sesion,
        IMemoryCache envios)
    {
        _ticketsApi = ticketsApi;
        _catalogosApi = catalogosApi;
        _usuariosApi = usuariosApi;
        _sesion = sesion;
        _envios = envios;
    }

    /// <summary>
    /// Identifica este formulario: si llega dos veces (doble clic, reenvío del navegador),
    /// el segundo envío lleva al ticket ya creado en lugar de crear otro.
    /// </summary>
    [BindProperty]
    public Guid ClaveEnvio { get; set; }

    [BindProperty]
    public CrearTicketDto Ticket { get; set; } = new() { PrioridadId = (int)PrioridadTipo.Media };

    [BindProperty]
    public List<IFormFile> Adjuntos { get; set; } = new();

    /// <summary>Solo Administrador y Agente eligen el cliente solicitante.</summary>
    public bool EligeSolicitante => _sesion.Rol != RolTipo.Cliente;

    public IReadOnlyList<SelectListItem> Clientes { get; private set; } = Array.Empty<SelectListItem>();
    public IReadOnlyList<SelectListItem> Categorias { get; private set; } = Array.Empty<SelectListItem>();
    public IReadOnlyList<CatalogoDto> Prioridades { get; private set; } = Array.Empty<CatalogoDto>();
    public string? MensajeError { get; private set; }
    public IReadOnlyList<string> Errores { get; private set; } = Array.Empty<string>();

    public async Task OnGetAsync()
    {
        ClaveEnvio = Guid.NewGuid();
        await CargarListasAsync();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!EligeSolicitante)
            Ticket.ClienteId = null;
        else if (Ticket.ClienteId is null)
            ModelState.AddModelError($"{nameof(Ticket)}.{nameof(CrearTicketDto.ClienteId)}", "Seleccione el cliente solicitante.");

        if (!ModelState.IsValid)
        {
            await CargarListasAsync();
            return Page();
        }

        var claveCache = $"crear-ticket:{_sesion.UsuarioId}:{ClaveEnvio}";
        if (_envios.TryGetValue(claveCache, out int idCreado))
            return RedirectToPage("./Detalle", new { id = idCreado });

        var resultado = await _ticketsApi.CrearAsync(Ticket, Adjuntos);
        if (!resultado.Exito)
        {
            MensajeError = resultado.Mensaje;
            Errores = resultado.Errores;
            await CargarListasAsync();
            return Page();
        }

        var id = resultado.Datos!.Id;
        _envios.Set(claveCache, id, TimeSpan.FromMinutes(10));
        TempData["MensajeExito"] = $"Ticket #{id} creado correctamente. Te avisaremos acá cuando haya novedades.";
        return RedirectToPage("./Detalle", new { id });
    }

    private async Task CargarListasAsync()
    {
        var tareaCategorias = _catalogosApi.ListarCategoriasAsync();
        var tareaPrioridades = _catalogosApi.ListarPrioridadesAsync();
        var tareaClientes = EligeSolicitante
            ? _usuariosApi.ListarClientesAsync()
            : Task.FromResult(ResultadoOperacion<IReadOnlyList<UsuarioDto>>.Ok(Array.Empty<UsuarioDto>()));
        await Task.WhenAll(tareaCategorias, tareaPrioridades, tareaClientes);

        var fallo = new ResultadoOperacion[] { tareaCategorias.Result, tareaPrioridades.Result, tareaClientes.Result }.FirstOrDefault(r => !r.Exito);
        if (fallo is not null && MensajeError is null)
            MensajeError = fallo.Mensaje;

        Categorias = (tareaCategorias.Result.Datos ?? Array.Empty<CatalogoDto>())
            .Select(c => new SelectListItem(c.Nombre, c.Id.ToString()))
            .ToList();
        Prioridades = tareaPrioridades.Result.Datos ?? Array.Empty<CatalogoDto>();
        Clientes = (tareaClientes.Result.Datos ?? Array.Empty<UsuarioDto>())
            .Select(c => new SelectListItem($"{c.Nombre} · {c.Correo}", c.Id.ToString()))
            .ToList();
    }
}
