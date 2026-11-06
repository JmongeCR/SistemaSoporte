using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Soporte.Abstracciones.Modelos;

namespace Soporte.DA.Configuraciones;

public class TicketArticuloKbConfiguracion : IEntityTypeConfiguration<TicketArticuloKb>
{
    public void Configure(EntityTypeBuilder<TicketArticuloKb> builder)
    {
        builder.ToTable("TicketArticuloKb");
        builder.HasKey(x => new { x.TicketId, x.ArticuloKbId });
        builder.Property(x => x.FechaVinculo).HasDefaultValueSql("SYSUTCDATETIME()");

        builder.HasOne(x => x.Ticket).WithMany().HasForeignKey(x => x.TicketId).OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(x => x.Articulo).WithMany(a => a.Tickets).HasForeignKey(x => x.ArticuloKbId).OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(x => x.VinculadoPor).WithMany().HasForeignKey(x => x.VinculadoPorId).OnDelete(DeleteBehavior.NoAction);
    }
}
