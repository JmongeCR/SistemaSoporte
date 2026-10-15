using Soporte.Abstracciones.Enumeraciones;
using Soporte.Abstracciones.Modelos;
using Soporte.Pruebas.Dobles;
using Soporte.Reglas.Reglas;

namespace Soporte.Pruebas.Reglas;

public class NotificacionReglasPruebas
{
    private readonly NotificacionReglas _reglas = new();

    [Fact]
    public void Equipo_cambia_estado_notifica_solo_al_cliente()
    {
        var ticket = Datos.TicketDeSofia(EstadoTicketTipo.EnProgreso);

        var pendientes = _reglas.PorCambioEstado(ticket, 1, 2, historialId: 15, Datos.Luis, conMensajePublico: false);

        var unica = Assert.Single(pendientes);
        Assert.Equal(Datos.Sofia.UsuarioId, unica.UsuarioId);
        Assert.Equal(TipoNotificacion.CambioEstado, unica.Tipo);
        Assert.Equal("Tu ticket #10 cambió a En progreso", unica.Mensaje);
        Assert.Equal("historial:15", unica.ClaveEvento);
    }

    [Fact]
    public void Resolucion_con_explicacion_emite_una_sola_notificacion_de_resuelto()
    {
        var pendientes = _reglas.PorCambioEstado(Datos.TicketDeSofia(EstadoTicketTipo.Resuelto), 2, 3, 20, Datos.Luis, conMensajePublico: true);

        var unica = Assert.Single(pendientes);
        Assert.Equal(TipoNotificacion.Resuelto, unica.Tipo);
        Assert.StartsWith("Tu ticket #10 fue resuelto", unica.Mensaje);
    }

    [Fact]
    public void Administrador_reabre_ticket_cerrado_notifica_reapertura_al_cliente()
    {
        var unica = Assert.Single(_reglas.PorCambioEstado(Datos.TicketDeSofia(EstadoTicketTipo.Abierto), 4, 1, 21, Datos.Admin, false));
        Assert.Equal(TipoNotificacion.Reabierto, unica.Tipo);
    }

    [Fact]
    public void Cliente_reabre_notifica_al_agente_y_no_a_si_mismo()
    {
        var unica = Assert.Single(_reglas.PorCambioEstado(Datos.TicketDeSofia(EstadoTicketTipo.Abierto), 3, 1, 22, Datos.Sofia, true));
        Assert.Equal(Datos.Luis.UsuarioId, unica.UsuarioId);
        Assert.Equal(TipoNotificacion.Reabierto, unica.Tipo);
    }

    [Fact]
    public void Cliente_reabre_ticket_sin_agente_no_notifica_a_nadie()
    {
        Assert.Empty(_reglas.PorCambioEstado(Datos.TicketDeSofia(EstadoTicketTipo.Abierto, agenteId: null), 3, 1, 23, Datos.Sofia, false));
    }

    [Fact]
    public void Mismo_estado_no_notifica()
    {
        Assert.Empty(_reglas.PorCambioEstado(Datos.TicketDeSofia(), 2, 2, 24, Datos.Luis, false));
    }

    [Fact]
    public void Nota_interna_no_notifica_a_nadie()
    {
        var nota = new Comentario { Id = 7, EsNotaInterna = true, Texto = "Dato interno" };
        Assert.Empty(_reglas.PorComentario(Datos.TicketDeSofia(), nota, Datos.Luis));
    }

    [Fact]
    public void Respuesta_publica_del_equipo_notifica_al_cliente()
    {
        var unica = Assert.Single(_reglas.PorComentario(Datos.TicketDeSofia(), new Comentario { Id = 8, Texto = "Revisando" }, Datos.Luis));
        Assert.Equal(Datos.Sofia.UsuarioId, unica.UsuarioId);
        Assert.Equal(TipoNotificacion.RespuestaEquipo, unica.Tipo);
        Assert.Equal("comentario:8", unica.ClaveEvento);
    }

    [Fact]
    public void Solicitud_de_informacion_tiene_su_propio_tipo()
    {
        var solicitud = new Comentario { Id = 9, Texto = "¿Qué navegador?", SolicitaInformacion = true };
        var unica = Assert.Single(_reglas.PorComentario(Datos.TicketDeSofia(), solicitud, Datos.Luis));
        Assert.Equal(TipoNotificacion.SolicitudInformacion, unica.Tipo);
        Assert.Equal("Necesitamos información adicional sobre tu ticket #10", unica.Mensaje);
    }

    [Fact]
    public void Respuesta_del_cliente_notifica_al_agente_asignado_y_no_al_cliente()
    {
        var unica = Assert.Single(_reglas.PorComentario(Datos.TicketDeSofia(), new Comentario { Id = 11, Texto = "Chrome" }, Datos.Sofia));
        Assert.Equal(Datos.Luis.UsuarioId, unica.UsuarioId);
        Assert.Equal(TipoNotificacion.RespuestaCliente, unica.Tipo);
    }
}
