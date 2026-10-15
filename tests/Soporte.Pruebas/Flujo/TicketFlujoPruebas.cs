using Soporte.Abstracciones.DTOs;
using Soporte.Abstracciones.Enumeraciones;
using Soporte.Abstracciones.Modelos;
using Soporte.Flujo.Servicios;
using Soporte.Pruebas.Dobles;
using Soporte.Reglas.Reglas;

namespace Soporte.Pruebas.Flujo;

public class TicketFlujoPruebas
{
    private readonly NotificacionRepositoryFalso _notificaciones = new();

    private (TicketFlujo Flujo, TicketRepositoryFalso Tickets) Crear(Ticket ticket)
    {
        var tickets = new TicketRepositoryFalso(ticket);
        var flujo = new TicketFlujo(
            tickets,
            new CatalogoRepositoryFalso(),
            new ComentarioRepositoryFalso(),
            _notificaciones,
            new UnidadDeTrabajoFalsa(),
            new TicketReglas(new UsuarioRepositoryFalso()),
            new ComentarioReglas(),
            new AdjuntoReglas(),
            new NotificacionReglas(),
            new AdjuntoFlujoFalso());
        return (flujo, tickets);
    }

    private static ActualizarTicketDto SinCambios(Ticket t) => new()
    {
        EstadoId = t.EstadoId,
        PrioridadId = t.PrioridadId,
        CategoriaId = t.CategoriaId,
        AgenteAsignadoId = t.AgenteAsignadoId,
        RowVersion = Convert.ToBase64String(t.RowVersion)
    };

    [Fact]
    public async Task Guardado_sin_cambios_no_persiste_ni_notifica()
    {
        var ticket = Datos.TicketDeSofia();
        var (flujo, tickets) = Crear(ticket);

        var resultado = await flujo.ActualizarAsync(ticket.Id, SinCambios(ticket), Datos.Luis);

        Assert.True(resultado.Exito);
        Assert.Equal(0, tickets.Actualizaciones);
        Assert.Empty(_notificaciones.Guardadas);
    }

    [Fact]
    public async Task Resolver_con_explicacion_genera_una_sola_notificacion_para_el_cliente()
    {
        var ticket = Datos.TicketDeSofia();
        var (flujo, _) = Crear(ticket);
        var cambios = SinCambios(ticket);
        cambios.EstadoId = (int)EstadoTicketTipo.Resuelto;
        cambios.MensajePublico = "Actualizamos la librería de PDF.";

        var resultado = await flujo.ActualizarAsync(ticket.Id, cambios, Datos.Luis);

        Assert.True(resultado.Exito);
        var unica = Assert.Single(_notificaciones.Guardadas);
        Assert.Equal(Datos.Sofia.UsuarioId, unica.UsuarioId);
        Assert.Equal(TipoNotificacion.Resuelto, unica.Tipo);
    }

    [Fact]
    public async Task Cambio_solo_de_prioridad_no_notifica_al_cliente()
    {
        var ticket = Datos.TicketDeSofia();
        var (flujo, tickets) = Crear(ticket);
        var cambios = SinCambios(ticket);
        cambios.PrioridadId = (int)PrioridadTipo.Alta;

        await flujo.ActualizarAsync(ticket.Id, cambios, Datos.Luis);

        Assert.Equal(1, tickets.Actualizaciones);
        Assert.Empty(_notificaciones.Guardadas);
    }

    [Fact]
    public async Task Otro_cliente_no_puede_modificar_el_ticket()
    {
        var ticket = Datos.TicketDeSofia(EstadoTicketTipo.Resuelto);
        var (flujo, tickets) = Crear(ticket);
        var cambios = SinCambios(ticket);
        cambios.EstadoId = (int)EstadoTicketTipo.Abierto;

        var resultado = await flujo.ActualizarAsync(ticket.Id, cambios, Datos.Diego);

        Assert.Equal(TipoError.NoEncontrado, resultado.TipoError);
        Assert.Equal(0, tickets.Actualizaciones);
    }

    [Fact]
    public async Task Tomar_un_ticket_abierto_notifica_el_cambio_a_en_progreso()
    {
        var ticket = Datos.TicketDeSofia(EstadoTicketTipo.Abierto, agenteId: null);
        var (flujo, _) = Crear(ticket);

        var resultado = await flujo.TomarAsync(ticket.Id, Datos.Luis);

        Assert.True(resultado.Exito);
        Assert.Equal("Tu ticket #10 cambió a En progreso", Assert.Single(_notificaciones.Guardadas).Mensaje);
    }
}
