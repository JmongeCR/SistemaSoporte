using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Soporte.Abstracciones.Modelos;

namespace Soporte.DA.Configuraciones;

public class UsuarioConfiguracion : IEntityTypeConfiguration<Usuario>
{
    public void Configure(EntityTypeBuilder<Usuario> builder)
    {
        builder.ToTable("Usuario");
        builder.HasKey(u => u.Id);
        builder.Property(u => u.Nombre).HasMaxLength(100).IsRequired();
        builder.Property(u => u.Correo).HasMaxLength(150).IsRequired();
        builder.Property(u => u.PasswordHash).HasMaxLength(255).IsRequired();
        builder.Property(u => u.Activo).HasDefaultValue(true);
        builder.Property(u => u.FechaCreacion).HasDefaultValueSql("SYSUTCDATETIME()");
        builder.Property(u => u.ContrasenaVersion).HasDefaultValue(0);

        builder.HasIndex(u => u.Correo).IsUnique();
        builder.HasIndex(u => u.RolId);

        builder.HasOne(u => u.Rol)
               .WithMany(r => r.Usuarios)
               .HasForeignKey(u => u.RolId);
    }
}
