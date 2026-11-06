using Markdig;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Soporte.Abstracciones.DTOs;
using Soporte.Web.Servicios;

namespace Soporte.Web.Pages.Ayuda;

public class ArticuloModel : PageModel
{
    private readonly IBaseConocimientoApiServicio _kb;
    private static readonly MarkdownPipeline Pipeline = new MarkdownPipelineBuilder()
        .UseAdvancedExtensions()
        .Build();

    public ArticuloModel(IBaseConocimientoApiServicio kb)
    {
        _kb = kb;
    }

    public ArticuloKbDto? Articulo { get; private set; }
    public string ContenidoHtml { get; private set; } = string.Empty;

    public async Task OnGetAsync(string slug)
    {
        var resultado = await _kb.ObtenerPorSlugAsync(slug);
        if (resultado.Exito && resultado.Datos is not null)
        {
            Articulo = resultado.Datos;
            ContenidoHtml = Markdown.ToHtml(Articulo.Contenido, Pipeline);
        }
    }
}
