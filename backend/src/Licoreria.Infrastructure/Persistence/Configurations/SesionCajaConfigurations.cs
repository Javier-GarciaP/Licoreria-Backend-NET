using Licoreria.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Licoreria.Infrastructure.Persistence.Configurations;

public class SesionCajaConfiguration : IEntityTypeConfiguration<SesionCaja>
{
    public void Configure(EntityTypeBuilder<SesionCaja> builder)
    {
        builder.ToTable("sesiones_caja");
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Estado).HasConversion<string>().HasMaxLength(20);
        builder.Property(s => s.FondoInicial).HasPrecision(18, 2);
        builder.Property(s => s.MontoEsperado).HasPrecision(18, 2);
        builder.Property(s => s.MontoContado).HasPrecision(18, 2);
        builder.Property(s => s.Descuadre).HasPrecision(18, 2);

        builder.HasOne(s => s.Usuario)
               .WithMany()
               .HasForeignKey(s => s.UsuarioId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}

public class MovimientoCajaConfiguration : IEntityTypeConfiguration<MovimientoCaja>
{
    public void Configure(EntityTypeBuilder<MovimientoCaja> builder)
    {
        builder.ToTable("movimientos_caja");
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Tipo).HasConversion<string>().HasMaxLength(20);
        builder.Property(m => m.Moneda).HasConversion<string>().HasMaxLength(10);
        builder.Property(m => m.Monto).HasPrecision(18, 2);
        builder.Property(m => m.Motivo).IsRequired().HasMaxLength(200);

        builder.HasOne(m => m.SesionCaja)
               .WithMany(s => s.Movimientos)
               .HasForeignKey(m => m.SesionCajaId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}

public class ArqueoDenominacionConfiguration : IEntityTypeConfiguration<ArqueoDenominacion>
{
    public void Configure(EntityTypeBuilder<ArqueoDenominacion> builder)
    {
        builder.ToTable("arqueos_denominacion");
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Subtotal).HasPrecision(18, 2);

        builder.HasOne(a => a.SesionCaja)
               .WithMany(s => s.Arqueos)
               .HasForeignKey(a => a.SesionCajaId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(a => a.Denominacion)
               .WithMany()
               .HasForeignKey(a => a.DenominacionId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
