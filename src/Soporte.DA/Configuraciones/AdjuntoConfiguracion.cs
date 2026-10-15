using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Soporte.Abstracciones.Modelos;

namespace Soporte.DA.Configuraciones;

public class AdjuntoConfiguracion : IEntityTypeConfiguration<Adjunto>
{
    public void Configure(EntityTypeBuilder<Adjunto> builder)
    {
        builder.ToTable("Adjunto", t =>
        {
            // Exactamente un padre: el ticket (HU-04) o el comentario (HU-11).
            t.HasCheckConstraint("CK_Adjunto_UnPadre",
                "([TicketId] IS NOT NULL AND [ComentarioId] IS NULL) OR ([TicketId] IS NULL AND [ComentarioId] IS NOT NULL)");
            t.HasCheckConstraint("CK_Adjunto_Tamano",
                $"[TamanoBytes] > 0 AND [TamanoBytes] <= {Adjunto.TamanoMaximoBytes}");
        });

        builder.HasKey(a => a.Id);
        builder.Property(a => a.NombreOriginal).HasMaxLength(255).IsRequired();
        builder.Property(a => a.TipoContenido).HasMaxLength(100).IsRequired();
        builder.Property(a => a.RutaAlmacenamiento).HasMaxLength(500).IsRequired();
        builder.Property(a => a.FechaSubida).HasDefaultValueSql("SYSUTCDATETIME()");

        builder.HasOne(a => a.Ticket)
               .WithMany(t => t.Adjuntos)
               .HasForeignKey(a => a.TicketId)
               .IsRequired(false);

        builder.HasOne(a => a.Comentario)
               .WithMany(c => c.Adjuntos)
               .HasForeignKey(a => a.ComentarioId)
               .IsRequired(false);

        builder.HasOne(a => a.Usuario)
               .WithMany()
               .HasForeignKey(a => a.UsuarioId);
    }
}
