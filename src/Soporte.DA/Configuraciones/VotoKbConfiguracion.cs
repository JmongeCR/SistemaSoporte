using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Soporte.Abstracciones.Modelos;

namespace Soporte.DA.Configuraciones;

public class VotoKbConfiguracion : IEntityTypeConfiguration<VotoKb>
{
    public void Configure(EntityTypeBuilder<VotoKb> builder)
    {
        builder.ToTable("VotoKb");
        builder.HasKey(x => new { x.ArticuloKbId, x.UsuarioId });
        builder.Property(x => x.FechaVoto).HasDefaultValueSql("SYSUTCDATETIME()");
        builder.HasOne(x => x.Articulo).WithMany().HasForeignKey(x => x.ArticuloKbId).OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(x => x.Usuario).WithMany().HasForeignKey(x => x.UsuarioId).OnDelete(DeleteBehavior.NoAction);
    }
}
