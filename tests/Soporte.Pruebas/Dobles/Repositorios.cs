using Soporte.Abstracciones.DTOs;
using Soporte.Abstracciones.Enumeraciones;
using Soporte.Abstracciones.Interfaces.DA;
using Soporte.Abstracciones.Interfaces.Flujo;
using Soporte.Abstracciones.Modelos;

namespace Soporte.Pruebas.Dobles;

/// <summary>Repositorio de tickets en memoria con un solo ticket.</summary>
public class TicketRepositoryFalso : ITicketRepository
{
    private int _siguienteId = 100;

    public TicketRepositoryFalso(Ticket ticket) => Ticket = ticket;

    public Ticket Ticket { get; }
    public int Actualizaciones { get; private set; }

    public Task<Ticket?> ObtenerPorIdAsync(int id) => Task.FromResult(id == Ticket.Id ? Ticket : null);

    public Task<bool> ActualizarAsync(Ticket ticket, byte[] rowVersionEsperado)
    {
        Actualizaciones++;
        // Simula los Id que asignaría la base al historial y comentarios nuevos.
        foreach (var h in ticket.Historial.Where(h => h.Id == 0)) h.Id = _siguienteId++;
        foreach (var c in ticket.Comentarios.Where(c => c.Id == 0)) { c.Id = _siguienteId++; c.TicketId = ticket.Id; }
        return Task.FromResult(true);
    }

    public Task<TicketDetalleDto?> ObtenerDetalleAsync(int id, bool incluirNotasInternas, int lectorId) =>
        Task.FromResult<TicketDetalleDto?>(new TicketDetalleDto { Id = Ticket.Id, EstadoId = Ticket.EstadoId });

    public Task<(bool Asignado, int? HistorialId)> TomarTicketAsync(int ticketId, int agenteId)
    {
        if (Ticket.AgenteAsignadoId is not null) return Task.FromResult((false, (int?)null));
        Ticket.AgenteAsignadoId = agenteId;
        Ticket.EstadoId = (int)EstadoTicketTipo.EnProgreso;
        return Task.FromResult((true, (int?)_siguienteId++));
    }

    public Task<ResultadoPaginado<TicketResumenDto>> ListarAsync(FiltroTicketsDto filtro, int? alcanceAgenteId, int? alcanceClienteId) =>
        throw new NotSupportedException();
    public Task<int> CrearAsync(Ticket ticket) => throw new NotSupportedException();
    public Task RegistrarActividadAsync(int ticketId, DateTime fechaUtc) => Task.CompletedTask;
}

public class NotificacionRepositoryFalso : INotificacionRepository
{
    public List<Notificacion> Guardadas { get; } = new();

    public Task<int> CrearAsync(IReadOnlyList<Notificacion> notificaciones)
    {
        var nuevas = notificaciones.Where(n => !Guardadas.Any(g => g.UsuarioId == n.UsuarioId && g.ClaveEvento == n.ClaveEvento)).ToList();
        foreach (var n in nuevas)
        {
            n.Id = Guardadas.Count + 1;
            Guardadas.Add(n);
        }
        return Task.FromResult(nuevas.Count);
    }

    public Task<IReadOnlyList<NotificacionDto>> ListarAsync(int usuarioId, bool soloNoLeidas, int cantidad) =>
        Task.FromResult<IReadOnlyList<NotificacionDto>>(Guardadas.Where(n => n.UsuarioId == usuarioId).Select(ADto).ToList());

    public Task<int> ContarNoLeidasAsync(int usuarioId) =>
        Task.FromResult(Guardadas.Count(n => n.UsuarioId == usuarioId && n.FechaLectura is null));

    public Task<NotificacionDto?> MarcarLeidaAsync(int id, int usuarioId)
    {
        var n = Guardadas.SingleOrDefault(x => x.Id == id && x.UsuarioId == usuarioId);
        if (n is not null) n.FechaLectura ??= DateTime.UtcNow;
        return Task.FromResult(n is null ? null : ADto(n));
    }

    public Task<int> MarcarTodasLeidasAsync(int usuarioId)
    {
        var pendientes = Guardadas.Where(n => n.UsuarioId == usuarioId && n.FechaLectura is null).ToList();
        pendientes.ForEach(n => n.FechaLectura = DateTime.UtcNow);
        return Task.FromResult(pendientes.Count);
    }

    private static NotificacionDto ADto(Notificacion n) =>
        new() { Id = n.Id, TicketId = n.TicketId, Tipo = n.Tipo, Mensaje = n.Mensaje, Leida = n.FechaLectura is not null };
}

public class UnidadDeTrabajoFalsa : IUnidadDeTrabajo
{
    public Task<T> EjecutarAsync<T>(Func<Task<T>> operacion) => operacion();
}

public class CatalogoRepositoryFalso : ICatalogoRepository
{
    public Task<bool> ExisteCategoriaActivaAsync(int categoriaId) => Task.FromResult(true);
    public Task<IReadOnlyList<CatalogoDto>> ListarCategoriasAsync() => throw new NotSupportedException();
    public Task<IReadOnlyList<CatalogoDto>> ListarPrioridadesAsync() => throw new NotSupportedException();
    public Task<IReadOnlyList<CatalogoDto>> ListarEstadosAsync() => throw new NotSupportedException();
    public Task<IReadOnlyList<CatalogoDto>> ListarRolesAsync() => throw new NotSupportedException();
    public Task<IReadOnlyList<Permiso>> ListarPermisosConRolesAsync() => throw new NotSupportedException();
}

public class ComentarioRepositoryFalso : IComentarioRepository
{
    public Task<IReadOnlyList<ComentarioDto>> ListarPorTicketAsync(int ticketId, bool incluirNotasInternas, int lectorId) =>
        Task.FromResult<IReadOnlyList<ComentarioDto>>(Array.Empty<ComentarioDto>());
    public Task<int> CrearAsync(Comentario comentario) => Task.FromResult(1);
    public Task MarcarLeidosAsync(int ticketId, int usuarioId, bool incluirNotasInternas) => Task.CompletedTask;
}

/// <summary>Usuarios del seed: 3 y 4 son agentes activos.</summary>
public class UsuarioRepositoryFalso : IUsuarioRepository
{
    public Task<Usuario?> ObtenerPorIdAsync(int id) => Task.FromResult<Usuario?>(id is 3 or 4
        ? new Usuario { Id = id, RolId = (int)RolTipo.Agente, Activo = true }
        : new Usuario { Id = id, RolId = (int)RolTipo.Cliente, Activo = true });
    public Task<IReadOnlyList<UsuarioDto>> ListarAsync(FiltroUsuariosDto filtro) => throw new NotSupportedException();
    public Task<Usuario?> ObtenerPorCorreoAsync(string correo) => throw new NotSupportedException();
    public Task<int> ContarPorRolAsync(int rolId) => throw new NotSupportedException();
    public Task ActualizarRolAsync(int usuarioId, int rolId) => throw new NotSupportedException();
    public Task ActualizarPasswordAsync(int usuarioId, string passwordHash) => throw new NotSupportedException();
}

public class AdjuntoFlujoFalso : IAdjuntoFlujo
{
    public Task<ResultadoOperacion<IReadOnlyList<AdjuntoDto>>> AgregarATicketAsync(int ticketId, IReadOnlyList<ArchivoEntranteDto> archivos, UsuarioContextoDto usuario) =>
        throw new NotSupportedException();
    public Task<ResultadoOperacion<IReadOnlyList<AdjuntoDto>>> AgregarAComentarioAsync(int ticketId, int comentarioId, IReadOnlyList<ArchivoEntranteDto> archivos, UsuarioContextoDto usuario) =>
        throw new NotSupportedException();
    public Task<ResultadoOperacion<ArchivoDescargaDto>> DescargarAsync(int adjuntoId, UsuarioContextoDto usuario) =>
        throw new NotSupportedException();
}
