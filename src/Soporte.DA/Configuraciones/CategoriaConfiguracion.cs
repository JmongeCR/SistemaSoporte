using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Soporte.Abstracciones.Modelos;

namespace Soporte.DA.Configuraciones;

public class CategoriaConfiguracion : IEntityTypeConfiguration<Categoria>
{
    public void Configure(EntityTypeBuilder<Categoria> builder)
    {
        builder.ToTable("Categoria");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Nombre).HasMaxLength(50).IsRequired();
        builder.Property(c => c.Activa).HasDefaultValue(true);
        builder.HasIndex(c => c.Nombre).IsUnique();
    }
}
