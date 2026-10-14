using Licoreria.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Licoreria.Infrastructure.Persistence.Configurations;

public class PagoConfiguration : IEntityTypeConfiguration<Pago>
{
    public void Configure(EntityTypeBuilder<Pago> builder)
    {
        builder.ToTable("pagos");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Monto).HasPrecision(18, 2);
        builder.Property(p => p.Propina).HasPrecision(18, 2);
        builder.Property(p => p.Moneda).HasConversion<string>().HasMaxLength(10);

        builder.HasOne(p => p.Venta)
               .WithMany(v => v.Pagos)
               .HasForeignKey(p => p.VentaId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(p => p.MetodoPago)
               .WithMany()
               .HasForeignKey(p => p.MetodoPagoId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
