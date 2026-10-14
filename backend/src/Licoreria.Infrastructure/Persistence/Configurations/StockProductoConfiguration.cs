using Licoreria.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Licoreria.Infrastructure.Persistence.Configurations;

public class StockProductoConfiguration : IEntityTypeConfiguration<StockProducto>
{
    public void Configure(EntityTypeBuilder<StockProducto> builder)
    {
        // Red de seguridad final: aunque la lógica de aplicación falle, la base de datos
        // impide existencia negativa o reservas que superen la existencia.
        builder.ToTable("stock_producto", tabla =>
        {
            tabla.HasCheckConstraint("ck_stock_cantidad_no_negativa", "\"Cantidad\" >= 0");
            tabla.HasCheckConstraint("ck_stock_reservada_no_negativa", "\"CantidadReservada\" >= 0");
            tabla.HasCheckConstraint("ck_stock_reservada_no_supera_cantidad", "\"CantidadReservada\" <= \"Cantidad\"");
        });
        builder.HasKey(s => s.Id);
        builder.HasIndex(s => s.VarianteId).IsUnique();

        builder.Property(s => s.Cantidad).HasPrecision(14, 3);
        builder.Property(s => s.CantidadReservada).HasPrecision(14, 3);
        builder.Property(s => s.StockMinimo).HasPrecision(14, 3);
        builder.Property(s => s.StockMaximo).HasPrecision(14, 3);
        builder.Ignore(s => s.BajoMinimo);
        builder.Ignore(s => s.Disponible);

        builder.HasOne(s => s.Variante)
               .WithMany()
               .HasForeignKey(s => s.VarianteId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
