using Licoreria.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Licoreria.Infrastructure.Persistence.Configurations;

public class MovimientoInventarioConfiguration : IEntityTypeConfiguration<MovimientoInventario>
{
    public void Configure(EntityTypeBuilder<MovimientoInventario> builder)
    {
        builder.ToTable("movimiento_inventario");
        builder.HasKey(m => m.Id);

        builder.Property(m => m.Tipo).HasConversion<string>().HasMaxLength(20);
        builder.Property(m => m.Cantidad).HasPrecision(14, 3);
        builder.Property(m => m.CostoUnitario).HasPrecision(18, 2);
        builder.Property(m => m.ReferenciaTipo).HasMaxLength(40);
        builder.Property(m => m.Motivo).HasMaxLength(200);
        builder.HasIndex(m => m.VarianteId);

        builder.HasOne(m => m.Variante)
               .WithMany()
               .HasForeignKey(m => m.VarianteId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
