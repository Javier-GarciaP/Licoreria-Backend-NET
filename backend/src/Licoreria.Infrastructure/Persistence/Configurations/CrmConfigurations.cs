using Licoreria.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Licoreria.Infrastructure.Persistence.Configurations;

public class ClienteConfiguration : IEntityTypeConfiguration<Cliente>
{
    public void Configure(EntityTypeBuilder<Cliente> builder)
    {
        builder.ToTable("clientes");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Nombre).IsRequired().HasMaxLength(120);
        builder.Property(c => c.Rif).HasMaxLength(20);
        builder.Property(c => c.Ci).HasMaxLength(20);
        builder.Property(c => c.Email).HasMaxLength(120);
        builder.Property(c => c.Telefono).HasMaxLength(30);
        builder.Property(c => c.Direccion).HasMaxLength(300);
        builder.HasIndex(c => c.Rif);
    }
}

public class PuntosMovimientoConfiguration : IEntityTypeConfiguration<PuntosMovimiento>
{
    public void Configure(EntityTypeBuilder<PuntosMovimiento> builder)
    {
        builder.ToTable("puntos_movimiento");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Tipo).HasConversion<string>().HasMaxLength(20);
        builder.Property(p => p.Motivo).IsRequired().HasMaxLength(200);
        builder.Property(p => p.ReferenciaTipo).HasMaxLength(40);

        builder.HasOne(p => p.Cliente)
               .WithMany(c => c.Movimientos)
               .HasForeignKey(p => p.ClienteId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}

public class CuentaPorCobrarConfiguration : IEntityTypeConfiguration<CuentaPorCobrar>
{
    public void Configure(EntityTypeBuilder<CuentaPorCobrar> builder)
    {
        builder.ToTable("cuentas_por_cobrar");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.MontoUSD).HasPrecision(18, 2);
        builder.Property(c => c.SaldoUSD).HasPrecision(18, 2);
        builder.Property(c => c.Estado).HasConversion<string>().HasMaxLength(20);

        builder.HasOne(c => c.Cliente)
               .WithMany()
               .HasForeignKey(c => c.ClienteId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
