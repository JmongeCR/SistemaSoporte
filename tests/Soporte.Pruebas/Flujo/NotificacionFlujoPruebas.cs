using Soporte.Abstracciones.Enumeraciones;
using Soporte.Abstracciones.Modelos;
using Soporte.Flujo.Servicios;
using Soporte.Pruebas.Dobles;

namespace Soporte.Pruebas.Flujo;

public class NotificacionFlujoPruebas
{
    private readonly NotificacionRepositoryFalso _repositorio = new();
    private readonly NotificacionFlujo _flujo;

    public NotificacionFlujoPruebas()
    {
        _flujo = new NotificacionFlujo(_repositorio);
        _repositorio.CrearAsync(new[]
        {
            new Notificacion { UsuarioId = Datos.Sofia.UsuarioId, TicketId = 10, Tipo = TipoNotificacion.RespuestaEquipo, Mensaje = "A", ClaveEvento = "comentario:1" },
            new Notificacion { UsuarioId = Datos.Sofia.UsuarioId, TicketId = 10, Tipo = TipoNotificacion.Resuelto, Mensaje = "B", ClaveEvento = "historial:2" }
        }).Wait();
    }

    [Fact]
    public async Task Un_usuario_no_puede_marcar_notificaciones_ajenas()
    {
        var resultado = await _flujo.MarcarLeidaAsync(1, Datos.Diego);

        Assert.Equal(TipoError.NoEncontrado, resultado.TipoError);
        Assert.Equal(2, (await _flujo.ObtenerResumenAsync(Datos.Sofia)).NoLeidas);
    }

    [Fact]
    public async Task Un_usuario_no_ve_notificaciones_ajenas()
    {
        Assert.Empty(await _flujo.ListarAsync(Datos.Diego, soloNoLeidas: false, cantidad: 20));
    }

    [Fact]
    public async Task Marcar_leida_devuelve_el_ticket_y_baja_el_contador()
    {
        var resultado = await _flujo.MarcarLeidaAsync(1, Datos.Sofia);

        Assert.True(resultado.Exito);
        Assert.Equal(10, resultado.Datos!.TicketId);
        Assert.Equal(1, (await _flujo.ObtenerResumenAsync(Datos.Sofia)).NoLeidas);
        Assert.Equal(0, (await _flujo.MarcarTodasLeidasAsync(Datos.Sofia)).NoLeidas);
    }

    [Fact]
    public async Task El_mismo_evento_no_se_notifica_dos_veces()
    {
        var repetida = new Notificacion { UsuarioId = Datos.Sofia.UsuarioId, TicketId = 10, Mensaje = "A", ClaveEvento = "comentario:1" };
        Assert.Equal(0, await _repositorio.CrearAsync(new[] { repetida }));
    }
}
