using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Soporte.Abstracciones.Modelos;

namespace Soporte.DA.Configuraciones;

public class HistorialTicketConfiguracion : IEntityTypeConfiguration<HistorialTicket>
{
    public void Configure(EntityTypeBuilder<HistorialTicket> builder)
    {
        builder.ToTable("HistorialTicket");
        builder.HasKey(h => h.Id);
        builder.Property(h => h.FechaCambio).HasDefaultValueSql("SYSUTCDATETIME()");

        builder.HasOne(h => h.Ticket)
               .WithMany(t => t.Historial)
               .HasForeignKey(h => h.TicketId);

        builder.HasOne(h => h.Usuario)
               .WithMany()
               .HasForeignKey(h => h.UsuarioId);

        builder.HasOne(h => h.EstadoAnterior)
               .WithMany()
               .HasForeignKey(h => h.EstadoAnteriorId)
               .IsRequired(false)
               .HasConstraintName("FK_HistorialTicket_EstadoAnterior");

        builder.HasOne(h => h.EstadoNuevo)
               .WithMany()
               .HasForeignKey(h => h.EstadoNuevoId)
               .HasConstraintName("FK_HistorialTicket_EstadoNuevo");

        // "Resueltos hoy" (HU-12) filtra por estado nuevo y fecha.
        builder.HasIndex(h => new { h.EstadoNuevoId, h.FechaCambio });
        builder.HasIndex(h => h.TicketId);
    }
}
