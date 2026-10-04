using Licoreria.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Licoreria.Infrastructure.Persistence.Configurations;

public class CuentaPorPagarConfiguration : IEntityTypeConfiguration<CuentaPorPagar>
{
    public void Configure(EntityTypeBuilder<CuentaPorPagar> builder)
    {
        builder.ToTable("cuentas_por_pagar");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.MontoUSD).HasPrecision(18, 2);
        builder.Property(c => c.SaldoUSD).HasPrecision(18, 2);
        builder.Property(c => c.Estado).HasConversion<string>().HasMaxLength(20);
        builder.HasIndex(c => c.Vencimiento);

        builder.HasOne(c => c.Proveedor)
               .WithMany()
               .HasForeignKey(c => c.ProveedorId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}

public class PagoProveedorConfiguration : IEntityTypeConfiguration<PagoProveedor>
{
    public void Configure(EntityTypeBuilder<PagoProveedor> builder)
    {
        builder.ToTable("pagos_proveedor");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Monto).HasPrecision(18, 2);
        builder.Property(p => p.Moneda).HasConversion<string>().HasMaxLength(10);
        builder.Property(p => p.Referencia).HasMaxLength(120);

        builder.HasOne(p => p.CuentaPorPagar)
               .WithMany(c => c.Pagos)
               .HasForeignKey(p => p.CuentaPorPagarId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
