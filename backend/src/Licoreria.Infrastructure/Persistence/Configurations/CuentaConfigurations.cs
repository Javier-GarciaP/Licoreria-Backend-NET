using Licoreria.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Licoreria.Infrastructure.Persistence.Configurations;

public class SesionMesaConfiguration : IEntityTypeConfiguration<SesionMesa>
{
    public void Configure(EntityTypeBuilder<SesionMesa> builder)
    {
        builder.ToTable("sesiones_mesa");
        builder.HasKey(s => s.Id);
        builder.Property(s => s.NombreMesa).IsRequired().HasMaxLength(60);
    }
}

public class CuentaConfiguration : IEntityTypeConfiguration<Cuenta>
{
    public void Configure(EntityTypeBuilder<Cuenta> builder)
    {
        builder.ToTable("cuentas");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Estado).HasConversion<string>().HasMaxLength(20);
        builder.Property(c => c.Total).HasPrecision(18, 2);
        builder.Property(c => c.TotalAbonado).HasPrecision(18, 2);
        builder.Ignore(c => c.Saldo);

        builder.HasIndex(c => c.SesionMesaId).IsUnique();
        builder.HasOne(c => c.SesionMesa)
               .WithOne(s => s.Cuenta)
               .HasForeignKey<Cuenta>(c => c.SesionMesaId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}

public class AbonoConfiguration : IEntityTypeConfiguration<Abono>
{
    public void Configure(EntityTypeBuilder<Abono> builder)
    {
        builder.ToTable("abonos");
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Monto).HasPrecision(18, 2);
        builder.Property(a => a.Moneda).HasConversion<string>().HasMaxLength(10);

        builder.HasOne(a => a.Cuenta)
               .WithMany(c => c.Abonos)
               .HasForeignKey(a => a.CuentaId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(a => a.MetodoPago)
               .WithMany()
               .HasForeignKey(a => a.MetodoPagoId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}

public class ComandaConfiguration : IEntityTypeConfiguration<Comanda>
{
    public void Configure(EntityTypeBuilder<Comanda> builder)
    {
        builder.ToTable("comandas");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Area).HasConversion<string>().HasMaxLength(20);
        builder.Property(c => c.Estado).HasConversion<string>().HasMaxLength(20);

        builder.HasOne(c => c.Cuenta)
               .WithMany(cu => cu.Comandas)
               .HasForeignKey(c => c.CuentaId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}

public class ComandaDetalleConfiguration : IEntityTypeConfiguration<ComandaDetalle>
{
    public void Configure(EntityTypeBuilder<ComandaDetalle> builder)
    {
        builder.ToTable("comanda_detalles");
        builder.HasKey(d => d.Id);
        builder.Property(d => d.Cantidad).HasPrecision(14, 3);
        builder.Property(d => d.PrecioUnitarioUSD).HasPrecision(18, 2);
        builder.Property(d => d.AreaDestino).HasConversion<string>().HasMaxLength(20);
        builder.Property(d => d.Estado).HasConversion<string>().HasMaxLength(20);
        builder.Ignore(d => d.SubtotalUSD);

        builder.HasOne(d => d.Comanda)
               .WithMany(c => c.Detalles)
               .HasForeignKey(d => d.ComandaId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(d => d.Variante)
               .WithMany()
               .HasForeignKey(d => d.VarianteId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
