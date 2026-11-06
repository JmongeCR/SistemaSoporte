using Markdig;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Soporte.Abstracciones.DTOs;
using Soporte.Abstracciones.Enumeraciones;
using Soporte.Web.Servicios;

namespace Soporte.Web.Pages.Ayuda;

public class ArticuloModel : PageModel
{
    private readonly IBaseConocimientoApiServicio _kb;
    private readonly IUsuarioSesion _sesion;
    private static readonly MarkdownPipeline Pipeline = new MarkdownPipelineBuilder()
        .UseAdvancedExtensions()
        .Build();

    public ArticuloModel(IBaseConocimientoApiServicio kb, IUsuarioSesion sesion)
    {
        _kb     = kb;
        _sesion = sesion;
    }

    public ArticuloKbDto?    Articulo      { get; private set; }
    public string            ContenidoHtml { get; private set; } = string.Empty;
    public VotoKbResultadoDto? Votos       { get; private set; }
    public IUsuarioSesion    Sesion        => _sesion;

    public bool PuedeGestionar =>
        _sesion.EstaAutenticado &&
        (_sesion.Rol == RolTipo.Administrador || _sesion.Rol == RolTipo.Agente);

    public async Task OnGetAsync(string slug)
    {
        var resultado = await _kb.ObtenerPorSlugAsync(slug);
        if (resultado.Exito && resultado.Datos is not null)
        {
            Articulo = resultado.Datos;
            ContenidoHtml = Markdown.ToHtml(Articulo.Contenido, Pipeline);
            var votosR = await _kb.ObtenerVotosAsync(Articulo.Id);
            if (votosR.Exito) Votos = votosR.Datos;
        }
    }

    public async Task<IActionResult> OnPostCambiarPublicadoAsync(int id, bool publicado, string slug)
    {
        await _kb.CambiarPublicadoAsync(id, publicado);
        return RedirectToPage(new { slug });
    }

    public async Task<IActionResult> OnPostVotarAsync(int articuloId, bool esUtil, string slug)
    {
        await _kb.VotarAsync(articuloId, esUtil);
        return RedirectToPage(new { slug });
    }

    public async Task<IActionResult> OnPostEliminarAsync(int id, string slug)
    {
        if (_sesion.Rol != RolTipo.Administrador) return Forbid();
        await _kb.EliminarAsync(id);
        return RedirectToPage("/Ayuda/Index");
    }
}
