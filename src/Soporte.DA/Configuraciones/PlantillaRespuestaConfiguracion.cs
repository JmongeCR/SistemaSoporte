using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Soporte.Abstracciones.Modelos;

namespace Soporte.DA.Configuraciones;

public class PlantillaRespuestaConfiguracion : IEntityTypeConfiguration<PlantillaRespuesta>
{
    public void Configure(EntityTypeBuilder<PlantillaRespuesta> builder)
    {
        builder.ToTable("PlantillaRespuesta");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Titulo).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Texto).HasMaxLength(4000).IsRequired();
        builder.Property(x => x.FechaCreacion).HasDefaultValueSql("SYSUTCDATETIME()");
        builder.Property(x => x.Compartida).HasDefaultValue(false);
        builder.HasOne(x => x.CreadoPor).WithMany().HasForeignKey(x => x.CreadoPorId).OnDelete(DeleteBehavior.NoAction);
    }
}
