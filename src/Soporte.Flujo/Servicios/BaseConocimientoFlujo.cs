using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using Soporte.Abstracciones.DTOs;
using Soporte.Abstracciones.Interfaces.DA;
using Soporte.Abstracciones.Interfaces.Flujo;
using Soporte.Abstracciones.Modelos;

namespace Soporte.Flujo.Servicios;

public class BaseConocimientoFlujo : IBaseConocimientoFlujo
{
    private readonly IBaseConocimientoRepository _repo;

    public BaseConocimientoFlujo(IBaseConocimientoRepository repo)
    {
        _repo = repo;
    }

    public Task<IReadOnlyList<ArticuloKbResumenDto>> ListarAsync(int? categoriaId = null, bool incluyeNoBublicados = false) =>
        _repo.ListarAsync(categoriaId, incluyeNoBublicados);

    public async Task<ResultadoOperacion<ArticuloKbDto>> ObtenerPorSlugAsync(string slug, bool incluyeNoBublicados = false)
    {
        var articulo = await _repo.ObtenerPorSlugAsync(slug, incluyeNoBublicados);
        if (articulo is null)
            return ResultadoOperacion<ArticuloKbDto>.NoEncontrado("Artículo no encontrado.");

        await _repo.IncrementarVistasAsync(articulo.Id);
        return ResultadoOperacion<ArticuloKbDto>.Ok(articulo);
    }

    public Task<IReadOnlyList<ArticuloKbResumenDto>> ListarPorTicketAsync(int ticketId) =>
        _repo.ListarPorTicketAsync(ticketId);

    public async Task<ResultadoOperacion> VincularAsync(int ticketId, int articuloId, UsuarioContextoDto usuario)
    {
        await _repo.VincularAsync(ticketId, articuloId, usuario.UsuarioId);
        return ResultadoOperacion.Ok();
    }

    public async Task<ResultadoOperacion> DesvincularAsync(int ticketId, int articuloId, UsuarioContextoDto usuario)
    {
        await _repo.DesvincularAsync(ticketId, articuloId);
        return ResultadoOperacion.Ok();
    }

    public async Task<ResultadoOperacion<ArticuloKbDto>> CambiarPublicadoAsync(int id, bool publicado, UsuarioContextoDto usuario)
    {
        if (usuario.Rol != Soporte.Abstracciones.Enumeraciones.RolTipo.Administrador &&
            usuario.Rol != Soporte.Abstracciones.Enumeraciones.RolTipo.Agente)
            return ResultadoOperacion<ArticuloKbDto>.SinPermiso("Solo agentes y administradores pueden publicar artículos.");

        await _repo.ActualizarPublicadoAsync(id, publicado);
        var articulo = await _repo.ObtenerPorSlugAsync(await SlugPorIdAsync(id), incluyeNoBublicados: true);
        return articulo is not null
            ? ResultadoOperacion<ArticuloKbDto>.Ok(articulo)
            : ResultadoOperacion<ArticuloKbDto>.NoEncontrado("Artículo no encontrado.");
    }

    public async Task<ResultadoOperacion<VotoKbResultadoDto>> VotarAsync(int articuloId, bool esUtil, UsuarioContextoDto usuario)
    {
        var resultado = await _repo.VotarAsync(articuloId, usuario.UsuarioId, esUtil);
        return ResultadoOperacion<VotoKbResultadoDto>.Ok(resultado);
    }

    public Task<VotoKbResultadoDto> ObtenerVotosAsync(int articuloId, int? usuarioId) =>
        _repo.ObtenerVotosAsync(articuloId, usuarioId);

    private async Task<string> SlugPorIdAsync(int id)
    {
        var a = await _repo.ObtenerPorIdAsync(id, incluyeNoBublicados: true);
        return a?.Slug ?? string.Empty;
    }

    public async Task<ResultadoOperacion<ArticuloKbDto>> CrearAsync(CrearArticuloKbDto dto, UsuarioContextoDto usuario)
    {
        var slug = await GenerarSlugUnicoAsync(dto.Titulo);

        var articulo = new ArticuloKb
        {
            Titulo        = dto.Titulo.Trim(),
            Contenido     = dto.Contenido.Trim(),
            Slug          = slug,
            CategoriaId   = dto.CategoriaId,
            CreadoPorId   = usuario.UsuarioId,
            FechaCreacion = DateTime.UtcNow,
            Publicado     = dto.Publicado
        };

        var resultado = await _repo.CrearAsync(articulo);
        return ResultadoOperacion<ArticuloKbDto>.Ok(resultado);
    }

    private async Task<string> GenerarSlugUnicoAsync(string titulo)
    {
        var base64 = Slugificar(titulo);
        var slug = base64;
        var i = 1;
        while (await _repo.ExisteSlugAsync(slug))
            slug = $"{base64}-{i++}";
        return slug;
    }

    private static string Slugificar(string texto)
    {
        var normalizado = texto.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder();
        foreach (var c in normalizado)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                sb.Append(c);
        }
        var sinAcentos = sb.ToString().Normalize(NormalizationForm.FormC).ToLowerInvariant();
        var slug = Regex.Replace(sinAcentos, @"[^a-z0-9\s-]", "");
        slug = Regex.Replace(slug, @"\s+", "-");
        slug = Regex.Replace(slug, @"-+", "-").Trim('-');
        return slug.Length > 200 ? slug[..200] : slug;
    }
}
