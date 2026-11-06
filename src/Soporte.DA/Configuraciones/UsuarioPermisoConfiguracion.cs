using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Soporte.Abstracciones.Modelos;

namespace Soporte.DA.Configuraciones;

public class UsuarioPermisoConfiguracion : IEntityTypeConfiguration<UsuarioPermiso>
{
    public void Configure(EntityTypeBuilder<UsuarioPermiso> builder)
    {
        builder.ToTable("UsuarioPermiso");
        builder.HasKey(x => new { x.UsuarioId, x.PermisoId });

        builder.HasOne(x => x.Usuario).WithMany().HasForeignKey(x => x.UsuarioId).OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(x => x.Permiso).WithMany().HasForeignKey(x => x.PermisoId).OnDelete(DeleteBehavior.NoAction);
    }
}
