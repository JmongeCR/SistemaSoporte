using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Soporte.Abstracciones.Modelos;

namespace Soporte.DA.Configuraciones;

public class ArticuloKbConfiguracion : IEntityTypeConfiguration<ArticuloKb>
{
    public void Configure(EntityTypeBuilder<ArticuloKb> builder)
    {
        builder.ToTable("ArticuloKb");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Titulo).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Slug).HasMaxLength(220).IsRequired();
        builder.HasIndex(x => x.Slug).IsUnique();
        builder.Property(x => x.Contenido).IsRequired();
        builder.Property(x => x.FechaCreacion).HasDefaultValueSql("SYSUTCDATETIME()");

        builder.HasOne(x => x.Categoria).WithMany().HasForeignKey(x => x.CategoriaId).OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(x => x.CreadoPor).WithMany().HasForeignKey(x => x.CreadoPorId).OnDelete(DeleteBehavior.NoAction);
    }
}
