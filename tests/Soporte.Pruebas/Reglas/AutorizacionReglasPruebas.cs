using Soporte.Abstracciones.DTOs;
using Soporte.Abstracciones.Enumeraciones;
using Soporte.Pruebas.Dobles;
using Soporte.Reglas.Reglas;

namespace Soporte.Pruebas.Reglas;

public class AutorizacionReglasPruebas
{
    private readonly TicketReglas _tickets = new(new UsuarioRepositoryFalso());
    private readonly ComentarioReglas _comentarios = new();

    [Fact]
    public void Un_cliente_no_puede_ver_el_ticket_de_otro_cliente()
    {
        Assert.True(_tickets.PuedeVer(Datos.TicketDeSofia(), Datos.Sofia));
        Assert.False(_tickets.PuedeVer(Datos.TicketDeSofia(), Datos.Diego));
    }

    [Fact]
    public void Un_agente_no_ve_tickets_asignados_a_otro_agente()
    {
        Assert.False(_tickets.PuedeVer(Datos.TicketDeSofia(), Datos.Maria));
        Assert.True(_tickets.PuedeVer(Datos.TicketDeSofia(agenteId: null), Datos.Maria));
    }

    [Fact]
    public void El_cliente_no_crea_notas_internas_ni_solicitudes_de_informacion()
    {
        var ticket = Datos.TicketDeSofia();
        Assert.Equal(TipoError.SinPermiso, _comentarios.ValidarNuevoComentario(ticket, new CrearComentarioDto { Texto = "x", EsNotaInterna = true }, Datos.Sofia).TipoError);
        Assert.Equal(TipoError.SinPermiso, _comentarios.ValidarNuevoComentario(ticket, new CrearComentarioDto { Texto = "x", SolicitaInformacion = true }, Datos.Sofia).TipoError);
    }

    [Fact]
    public void Una_solicitud_de_informacion_no_puede_ser_nota_interna()
    {
        var dto = new CrearComentarioDto { Texto = "x", SolicitaInformacion = true, EsNotaInterna = true };
        Assert.False(_comentarios.ValidarNuevoComentario(Datos.TicketDeSofia(), dto, Datos.Luis).Exito);
    }

    [Fact]
    public void Ticket_cerrado_no_admite_respuestas_publicas_pero_si_notas_internas()
    {
        var cerrado = Datos.TicketDeSofia(EstadoTicketTipo.Cerrado);
        Assert.False(_comentarios.ValidarNuevoComentario(cerrado, new CrearComentarioDto { Texto = "Hola" }, Datos.Sofia).Exito);
        Assert.True(_comentarios.ValidarNuevoComentario(cerrado, new CrearComentarioDto { Texto = "Registro", EsNotaInterna = true }, Datos.Luis).Exito);
    }

    [Fact]
    public void El_cliente_solo_reabre_un_ticket_resuelto()
    {
        Assert.True(_tickets.ValidarTransicionEstado(EstadoTicketTipo.Resuelto, EstadoTicketTipo.Abierto, RolTipo.Cliente).Exito);
        Assert.False(_tickets.ValidarTransicionEstado(EstadoTicketTipo.Abierto, EstadoTicketTipo.Resuelto, RolTipo.Cliente).Exito);
        Assert.False(_tickets.ValidarTransicionEstado(EstadoTicketTipo.Cerrado, EstadoTicketTipo.Abierto, RolTipo.Cliente).Exito);
    }
}
