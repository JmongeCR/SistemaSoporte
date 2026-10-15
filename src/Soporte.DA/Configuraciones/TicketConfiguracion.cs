using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Soporte.Abstracciones.Modelos;

namespace Soporte.DA.Configuraciones;

public class TicketConfiguracion : IEntityTypeConfiguration<Ticket>
{
    public void Configure(EntityTypeBuilder<Ticket> builder)
    {
        builder.ToTable("Ticket");
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Asunto).HasMaxLength(150).IsRequired();
        builder.Property(t => t.Descripcion).HasMaxLength(4000).IsRequired();
        builder.Property(t => t.FechaCreacion).HasDefaultValueSql("SYSUTCDATETIME()");
        builder.Property(t => t.RowVersion).IsRowVersion();

        builder.HasOne(t => t.Categoria).WithMany().HasForeignKey(t => t.CategoriaId);
        builder.HasOne(t => t.Prioridad).WithMany().HasForeignKey(t => t.PrioridadId);
        builder.HasOne(t => t.Estado).WithMany().HasForeignKey(t => t.EstadoId);

        // Dos relaciones hacia Usuario: se nombran explícitamente para que EF no las confunda.
        builder.HasOne(t => t.Cliente)
               .WithMany()
               .HasForeignKey(t => t.ClienteId)
               .HasConstraintName("FK_Ticket_Cliente");

        builder.HasOne(t => t.AgenteAsignado)
               .WithMany()
               .HasForeignKey(t => t.AgenteAsignadoId)
               .IsRequired(false)
               .HasConstraintName("FK_Ticket_AgenteAsignado");

        // Índices para los filtros de HU-05 y las métricas de HU-12/HU-13.
        builder.HasIndex(t => t.EstadoId);
        builder.HasIndex(t => t.AgenteAsignadoId);
        builder.HasIndex(t => t.ClienteId);
        builder.HasIndex(t => t.PrioridadId);
        builder.HasIndex(t => t.CategoriaId);
    }
}
