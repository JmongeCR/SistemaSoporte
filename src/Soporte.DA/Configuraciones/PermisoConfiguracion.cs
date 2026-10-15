using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Soporte.Abstracciones.Modelos;

namespace Soporte.DA.Configuraciones;

public class PermisoConfiguracion : IEntityTypeConfiguration<Permiso>
{
    public void Configure(EntityTypeBuilder<Permiso> builder)
    {
        builder.ToTable("Permiso");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Codigo).HasMaxLength(50).IsRequired();
        builder.Property(p => p.Descripcion).HasMaxLength(200).IsRequired();
        builder.Property(p => p.Grupo).HasMaxLength(50).IsRequired();
        builder.HasIndex(p => p.Codigo).IsUnique();
    }
}
