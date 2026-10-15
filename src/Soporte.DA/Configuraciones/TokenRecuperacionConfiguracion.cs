using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Soporte.Abstracciones.Modelos;

namespace Soporte.DA.Configuraciones;

public class TokenRecuperacionConfiguracion : IEntityTypeConfiguration<TokenRecuperacion>
{
    public void Configure(EntityTypeBuilder<TokenRecuperacion> builder)
    {
        builder.ToTable("TokenRecuperacion");
        builder.HasKey(t => t.Id);
        builder.Property(t => t.TokenHash).HasMaxLength(128).IsRequired();
        builder.Property(t => t.FechaCreacion).HasDefaultValueSql("SYSUTCDATETIME()");
        builder.Property(t => t.Usado).HasDefaultValue(false);
        builder.HasIndex(t => t.TokenHash).IsUnique();

        builder.HasOne(t => t.Usuario)
               .WithMany()
               .HasForeignKey(t => t.UsuarioId);
    }
}
