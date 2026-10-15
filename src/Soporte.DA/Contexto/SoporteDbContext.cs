using Microsoft.EntityFrameworkCore;
using Soporte.Abstracciones.Modelos;

namespace Soporte.DA.Contexto;

/// <summary>
/// Mapea el esquema definido en database/ (el script SQL es la fuente de verdad; no se usan migraciones).
/// Los resultados de procedimientos almacenados se leen con Database.SqlQuery&lt;T&gt;, sin mapear entidades extra.
/// </summary>
public class SoporteDbContext : DbContext
{
    public SoporteDbContext(DbContextOptions<SoporteDbContext> options) : base(options)
    {
    }

    public DbSet<Rol> Roles => Set<Rol>();
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Categoria> Categorias => Set<Categoria>();
    public DbSet<EstadoTicket> EstadosTicket => Set<EstadoTicket>();
    public DbSet<Prioridad> Prioridades => Set<Prioridad>();
    public DbSet<Ticket> Tickets => Set<Ticket>();
    public DbSet<Comentario> Comentarios => Set<Comentario>();
    public DbSet<Adjunto> Adjuntos => Set<Adjunto>();
    public DbSet<LecturaComentario> LecturasComentario => Set<LecturaComentario>();
    public DbSet<HistorialTicket> HistorialTickets => Set<HistorialTicket>();
    public DbSet<TokenRecuperacion> TokensRecuperacion => Set<TokenRecuperacion>();
    public DbSet<Permiso> Permisos => Set<Permiso>();
    public DbSet<RolPermiso> RolesPermisos => Set<RolPermiso>();
    public DbSet<Notificacion> Notificaciones => Set<Notificacion>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SoporteDbContext).Assembly);

        // Los tickets y usuarios no se borran físicamente; evitar cascadas también impide
        // el error de SQL Server "multiple cascade paths" (Ticket → Comentario → Adjunto).
        foreach (var fk in modelBuilder.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()))
        {
            if (fk.DeleteBehavior == DeleteBehavior.Cascade && !fk.IsOwnership)
                fk.DeleteBehavior = DeleteBehavior.NoAction;
        }
    }
}
