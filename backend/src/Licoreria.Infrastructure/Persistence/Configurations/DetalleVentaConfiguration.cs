using Licoreria.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Licoreria.Infrastructure.Persistence.Configurations;

public class DetalleVentaConfiguration : IEntityTypeConfiguration<DetalleVenta>
{
    public void Configure(EntityTypeBuilder<DetalleVenta> builder)
    {
        builder.ToTable("detalles_venta");
        builder.HasKey(d => d.Id);
        builder.Property(d => d.Cantidad).HasPrecision(14, 3);
        builder.Property(d => d.PrecioUnitarioUSD).HasPrecision(18, 2);
        builder.Property(d => d.DescuentoUSD).HasPrecision(18, 2);
        builder.Ignore(d => d.SubtotalUSD);

        builder.HasOne(d => d.Venta)
               .WithMany(v => v.Detalles)
               .HasForeignKey(d => d.VentaId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(d => d.Variante)
               .WithMany()
               .HasForeignKey(d => d.VarianteId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
