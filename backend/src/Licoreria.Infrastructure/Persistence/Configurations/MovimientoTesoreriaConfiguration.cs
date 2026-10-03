using Licoreria.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Licoreria.Infrastructure.Persistence.Configurations;

public class MovimientoTesoreriaConfiguration : IEntityTypeConfiguration<MovimientoTesoreria>
{
    public void Configure(EntityTypeBuilder<MovimientoTesoreria> builder)
    {
        builder.ToTable("movimientos_tesoreria");
        builder.HasKey(m => m.Id);

        builder.Property(m => m.Tipo).HasConversion<string>().HasMaxLength(20);
        builder.Property(m => m.Moneda).HasConversion<string>().HasMaxLength(10);
        builder.Property(m => m.Monto).HasPrecision(18, 2);
        builder.Property(m => m.Motivo).IsRequired().HasMaxLength(200);
        builder.Property(m => m.ReferenciaTipo).HasMaxLength(40);
    }
}
