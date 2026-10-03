using Licoreria.Domain.Entities;
using Licoreria.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Licoreria.Infrastructure.Persistence.Configurations;

public class PrecioProductoConfiguration : IEntityTypeConfiguration<PrecioProducto>
{
    public void Configure(EntityTypeBuilder<PrecioProducto> builder)
    {
        builder.ToTable("precios_producto");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Moneda).HasConversion<string>().HasMaxLength(10);
        builder.Property(p => p.Precio).HasPrecision(18, 2);
        builder.HasIndex(p => new { p.VarianteId, p.ListaPrecioId, p.Moneda }).IsUnique();

        builder.HasOne(p => p.Variante)
               .WithMany(v => v.Precios)
               .HasForeignKey(p => p.VarianteId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(p => p.ListaPrecio)
               .WithMany(l => l.Precios)
               .HasForeignKey(p => p.ListaPrecioId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasData(
            new { Id = Guid.Parse("90000000-0000-0000-0000-000000000001"), VarianteId = Guid.Parse("30000000-0000-0000-0000-000000000001"), ListaPrecioId = SeedData.ListaDetal, Moneda = Moneda.USD, Precio = 12.00m, Activo = true, CreatedAt = SeedData.Fecha, LastModifiedAt = (DateTime?)null, IsDeleted = false },
            new { Id = Guid.Parse("90000000-0000-0000-0000-000000000002"), VarianteId = Guid.Parse("30000000-0000-0000-0000-000000000002"), ListaPrecioId = SeedData.ListaDetal, Moneda = Moneda.USD, Precio = 38.00m, Activo = true, CreatedAt = SeedData.Fecha, LastModifiedAt = (DateTime?)null, IsDeleted = false },
            new { Id = Guid.Parse("90000000-0000-0000-0000-000000000003"), VarianteId = Guid.Parse("30000000-0000-0000-0000-000000000003"), ListaPrecioId = SeedData.ListaDetal, Moneda = Moneda.USD, Precio = 2.50m, Activo = true, CreatedAt = SeedData.Fecha, LastModifiedAt = (DateTime?)null, IsDeleted = false },
            new { Id = Guid.Parse("90000000-0000-0000-0000-000000000004"), VarianteId = Guid.Parse("30000000-0000-0000-0000-000000000004"), ListaPrecioId = SeedData.ListaDetal, Moneda = Moneda.USD, Precio = 3.00m, Activo = true, CreatedAt = SeedData.Fecha, LastModifiedAt = (DateTime?)null, IsDeleted = false },
            new { Id = Guid.Parse("90000000-0000-0000-0000-000000000005"), VarianteId = Guid.Parse("30000000-0000-0000-0000-000000000005"), ListaPrecioId = SeedData.ListaDetal, Moneda = Moneda.USD, Precio = 2.00m, Activo = true, CreatedAt = SeedData.Fecha, LastModifiedAt = (DateTime?)null, IsDeleted = false }
        );
    }
}
