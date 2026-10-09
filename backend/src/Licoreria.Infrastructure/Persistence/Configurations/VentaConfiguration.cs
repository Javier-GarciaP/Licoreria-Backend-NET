using Licoreria.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Licoreria.Infrastructure.Persistence.Configurations;

public class VentaConfiguration : IEntityTypeConfiguration<Venta>
{
    public void Configure(EntityTypeBuilder<Venta> builder)
    {
        builder.ToTable("ventas");
        builder.HasKey(v => v.Id);

        builder.Property(v => v.TasaCambio).HasPrecision(18, 4);
        builder.Property(v => v.SubtotalUSD).HasPrecision(18, 2);
        builder.Property(v => v.DescuentoUSD).HasPrecision(18, 2);
        builder.Property(v => v.TotalUSD).HasPrecision(18, 2);
        builder.Property(v => v.TotalBS).HasPrecision(18, 2);
        builder.Property(v => v.Estado).HasConversion<string>().HasMaxLength(20);

        builder.HasOne(v => v.Usuario)
               .WithMany()
               .HasForeignKey(v => v.UsuarioId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(v => v.Fecha);
        builder.HasIndex(v => v.SesionCajaId);
    }
}
