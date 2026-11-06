using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Soporte.Abstracciones.DTOs;
using Soporte.Web.Servicios;

namespace Soporte.Web.Pages.Ayuda;

public class IndexModel : PageModel
{
    private readonly IBaseConocimientoApiServicio _kb;

    public IndexModel(IBaseConocimientoApiServicio kb)
    {
        _kb = kb;
    }

    public IReadOnlyList<ArticuloKbResumenDto> Articulos { get; private set; } = Array.Empty<ArticuloKbResumenDto>();

    [BindProperty(SupportsGet = true)]
    public int? CategoriaId { get; set; }

    public async Task OnGetAsync()
    {
        var resultado = await _kb.ListarAsync(CategoriaId);
        if (resultado.Exito) Articulos = resultado.Datos!;
    }
}
