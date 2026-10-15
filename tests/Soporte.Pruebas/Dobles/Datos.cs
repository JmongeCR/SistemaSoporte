using Soporte.Abstracciones.DTOs;
using Soporte.Abstracciones.Enumeraciones;
using Soporte.Abstracciones.Modelos;

namespace Soporte.Pruebas.Dobles;

/// <summary>Usuarios y tickets de prueba (mismos Id que el seed: 3 = Luis agente, 5 = Sofía, 6 = Diego).</summary>
public static class Datos
{
    public static readonly UsuarioContextoDto Admin = new() { UsuarioId = 1, Rol = RolTipo.Administrador };
    public static readonly UsuarioContextoDto Luis = new() { UsuarioId = 3, Rol = RolTipo.Agente };
    public static readonly UsuarioContextoDto Maria = new() { UsuarioId = 4, Rol = RolTipo.Agente };
    public static readonly UsuarioContextoDto Sofia = new() { UsuarioId = 5, Rol = RolTipo.Cliente };
    public static readonly UsuarioContextoDto Diego = new() { UsuarioId = 6, Rol = RolTipo.Cliente };

    /// <summary>Ticket de Sofía atendido por Luis.</summary>
    public static Ticket TicketDeSofia(EstadoTicketTipo estado = EstadoTicketTipo.EnProgreso, int? agenteId = 3) => new()
    {
        Id = 10,
        Asunto = "No puedo exportar facturas",
        Descripcion = "La exportación a PDF queda en blanco.",
        CategoriaId = 2,
        PrioridadId = 2,
        EstadoId = (int)estado,
        ClienteId = Sofia.UsuarioId,
        AgenteAsignadoId = agenteId,
        RowVersion = new byte[] { 0, 0, 0, 0, 0, 0, 0, 1 }
    };
}
