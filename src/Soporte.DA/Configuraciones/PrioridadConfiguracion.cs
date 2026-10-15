using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Soporte.Abstracciones.Modelos;

namespace Soporte.DA.Configuraciones;

public class PrioridadConfiguracion : IEntityTypeConfiguration<Prioridad>
{
    public void Configure(EntityTypeBuilder<Prioridad> builder)
    {
        builder.ToTable("Prioridad", t => t.HasCheckConstraint("CK_Prioridad_HorasSla", "[HorasSla] > 0"));
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).ValueGeneratedNever();
        builder.Property(p => p.Nombre).HasMaxLength(20).IsRequired();
        builder.HasIndex(p => p.Nombre).IsUnique();
    }
}
