using Licoreria.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Licoreria.Infrastructure.Persistence.Configurations;

public class StockProductoConfiguration : IEntityTypeConfiguration<StockProducto>
{
    public void Configure(EntityTypeBuilder<StockProducto> builder)
    {
        builder.ToTable("stock_producto");
        builder.HasKey(s => s.Id);
        builder.HasIndex(s => s.VarianteId).IsUnique();

        builder.Property(s => s.Cantidad).HasPrecision(14, 3);
        builder.Property(s => s.CantidadReservada).HasPrecision(14, 3);
        builder.Property(s => s.StockMinimo).HasPrecision(14, 3);
        builder.Property(s => s.StockMaximo).HasPrecision(14, 3);
        builder.Ignore(s => s.BajoMinimo);

        builder.HasOne(s => s.Variante)
               .WithMany()
               .HasForeignKey(s => s.VarianteId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
