using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Soporte.Abstracciones.Modelos;

namespace Soporte.DA.Configuraciones;

public class LecturaComentarioConfiguracion : IEntityTypeConfiguration<LecturaComentario>
{
    public void Configure(EntityTypeBuilder<LecturaComentario> builder)
    {
        builder.ToTable("LecturaComentario");
        builder.HasKey(l => new { l.ComentarioId, l.UsuarioId });
        builder.Property(l => l.FechaLectura).HasDefaultValueSql("SYSUTCDATETIME()");

        builder.HasOne(l => l.Comentario)
               .WithMany(c => c.Lecturas)
               .HasForeignKey(l => l.ComentarioId);

        builder.HasOne(l => l.Usuario)
               .WithMany()
               .HasForeignKey(l => l.UsuarioId);
    }
}
