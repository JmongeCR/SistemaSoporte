using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Soporte.Abstracciones.Modelos;

namespace Soporte.DA.Configuraciones;

public class EstadoTicketConfiguracion : IEntityTypeConfiguration<EstadoTicket>
{
    public void Configure(EntityTypeBuilder<EstadoTicket> builder)
    {
        builder.ToTable("EstadoTicket");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedNever();
        builder.Property(e => e.Nombre).HasMaxLength(30).IsRequired();
        builder.HasIndex(e => e.Nombre).IsUnique();
    }
}
