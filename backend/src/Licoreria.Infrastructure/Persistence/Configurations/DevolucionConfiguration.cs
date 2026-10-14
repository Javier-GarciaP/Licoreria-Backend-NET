using Licoreria.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Licoreria.Infrastructure.Persistence.Configurations;

public class DevolucionConfiguration : IEntityTypeConfiguration<Devolucion>
{
    public void Configure(EntityTypeBuilder<Devolucion> builder)
    {
        builder.ToTable("devoluciones");
        builder.HasKey(d => d.Id);
        builder.Property(d => d.Motivo).IsRequired().HasMaxLength(200);
        builder.Property(d => d.MontoUSD).HasPrecision(18, 2);

        builder.HasOne(d => d.Venta)
               .WithMany(v => v.Devoluciones)
               .HasForeignKey(d => d.VentaId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}

public class DevolucionDetalleConfiguration : IEntityTypeConfiguration<DevolucionDetalle>
{
    public void Configure(EntityTypeBuilder<DevolucionDetalle> builder)
    {
        builder.ToTable("devolucion_detalles");
        builder.HasKey(d => d.Id);
        builder.Property(d => d.Cantidad).HasPrecision(14, 3);
        builder.Property(d => d.PrecioUnitarioUSD).HasPrecision(18, 2);

        builder.HasOne(d => d.Devolucion)
               .WithMany(v => v.Detalles)
               .HasForeignKey(d => d.DevolucionId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(d => d.Variante)
               .WithMany()
               .HasForeignKey(d => d.VarianteId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
