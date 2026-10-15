using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Soporte.Abstracciones.Modelos;

namespace Soporte.DA.Configuraciones;

public class ComentarioConfiguracion : IEntityTypeConfiguration<Comentario>
{
    public void Configure(EntityTypeBuilder<Comentario> builder)
    {
        builder.ToTable("Comentario");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Texto).HasMaxLength(4000).IsRequired();
        builder.Property(c => c.EsNotaInterna).HasDefaultValue(false);
        builder.Property(c => c.SolicitaInformacion).HasDefaultValue(false);
        builder.Property(c => c.FechaCreacion).HasDefaultValueSql("SYSUTCDATETIME()");

        builder.HasOne(c => c.Ticket)
               .WithMany(t => t.Comentarios)
               .HasForeignKey(c => c.TicketId);

        builder.HasOne(c => c.Usuario)
               .WithMany()
               .HasForeignKey(c => c.UsuarioId);

        builder.HasIndex(c => new { c.TicketId, c.FechaCreacion });
    }
}
