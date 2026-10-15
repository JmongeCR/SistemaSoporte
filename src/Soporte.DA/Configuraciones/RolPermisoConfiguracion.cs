using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Soporte.Abstracciones.Modelos;

namespace Soporte.DA.Configuraciones;

public class RolPermisoConfiguracion : IEntityTypeConfiguration<RolPermiso>
{
    public void Configure(EntityTypeBuilder<RolPermiso> builder)
    {
        builder.ToTable("RolPermiso");
        builder.HasKey(rp => new { rp.RolId, rp.PermisoId });

        builder.HasOne(rp => rp.Rol)
               .WithMany(r => r.Permisos)
               .HasForeignKey(rp => rp.RolId);

        builder.HasOne(rp => rp.Permiso)
               .WithMany(p => p.Roles)
               .HasForeignKey(rp => rp.PermisoId);
    }
}
