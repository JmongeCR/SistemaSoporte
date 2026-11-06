using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Soporte.Abstracciones.Modelos;

namespace Soporte.DA.Configuraciones;

public class CalificacionCsatConfiguracion : IEntityTypeConfiguration<CalificacionCsat>
{
    public void Configure(EntityTypeBuilder<CalificacionCsat> builder)
    {
        builder.ToTable("CalificacionCsat");
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => new { x.TicketId, x.ClienteId }).IsUnique();
        builder.Property(x => x.Comentario).HasMaxLength(500);
        builder.Property(x => x.FechaCalificacion).HasDefaultValueSql("SYSUTCDATETIME()");
        builder.HasOne(x => x.Ticket).WithMany().HasForeignKey(x => x.TicketId).OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(x => x.Cliente).WithMany().HasForeignKey(x => x.ClienteId).OnDelete(DeleteBehavior.NoAction);
    }
}
