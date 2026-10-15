using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Soporte.Abstracciones.Modelos;

namespace Soporte.DA.Configuraciones;

public class NotificacionConfiguracion : IEntityTypeConfiguration<Notificacion>
{
    public void Configure(EntityTypeBuilder<Notificacion> builder)
    {
        builder.ToTable("Notificacion");
        builder.HasKey(n => n.Id);
        builder.Property(n => n.Tipo).HasConversion<byte>();
        builder.Property(n => n.Mensaje).HasMaxLength(300).IsRequired();
        builder.Property(n => n.ClaveEvento).HasMaxLength(100).IsRequired();
        builder.Property(n => n.FechaCreacion).HasDefaultValueSql("SYSUTCDATETIME()");

        builder.HasIndex(n => new { n.UsuarioId, n.ClaveEvento }).IsUnique();

        builder.HasOne(n => n.Usuario).WithMany().HasForeignKey(n => n.UsuarioId);
        builder.HasOne(n => n.Ticket).WithMany().HasForeignKey(n => n.TicketId);
    }
}
