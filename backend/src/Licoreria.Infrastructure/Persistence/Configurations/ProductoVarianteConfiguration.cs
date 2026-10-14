using Licoreria.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Licoreria.Infrastructure.Persistence.Configurations;

public class ProductoVarianteConfiguration : IEntityTypeConfiguration<ProductoVariante>
{
    public void Configure(EntityTypeBuilder<ProductoVariante> builder)
    {
        builder.ToTable("producto_variantes");
        builder.HasKey(v => v.Id);

        builder.Property(v => v.Nombre).IsRequired().HasMaxLength(120);
        builder.Property(v => v.Sku).IsRequired().HasMaxLength(50);
        builder.HasIndex(v => v.Sku).IsUnique();
        builder.Property(v => v.PrecioCompraUSD).HasPrecision(18, 2);
        builder.Property(v => v.PrecioVentaUSD).HasPrecision(18, 2);
        builder.Property(v => v.EsBase).IsRequired().HasDefaultValue(false);

        builder.HasOne(v => v.Producto)
               .WithMany(p => p.Variantes)
               .HasForeignKey(v => v.ProductoId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(v => v.UnidadMedida)
               .WithMany(u => u.Variantes)
               .HasForeignKey(v => v.UnidadMedidaId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasData(
            new { Id = Guid.Parse("30000000-0000-0000-0000-000000000001"), ProductoId = Guid.Parse("10000000-0000-0000-0000-000000000001"), Nombre = "Botella 0.75L", Sku = "LIC-RON-0001", PrecioCompraUSD = 8.50m, PrecioVentaUSD = 12.00m, Activo = true, EsBase = true, UnidadMedidaId = SeedData.UnidadBotella, CreatedAt = SeedData.Fecha, LastModifiedAt = (DateTime?)null, IsDeleted = false },
            new { Id = Guid.Parse("30000000-0000-0000-0000-000000000002"), ProductoId = Guid.Parse("10000000-0000-0000-0000-000000000002"), Nombre = "Botella 0.75L", Sku = "LIC-WHI-0002", PrecioCompraUSD = 28.00m, PrecioVentaUSD = 38.00m, Activo = true, EsBase = true, UnidadMedidaId = SeedData.UnidadBotella, CreatedAt = SeedData.Fecha, LastModifiedAt = (DateTime?)null, IsDeleted = false },
            new { Id = Guid.Parse("30000000-0000-0000-0000-000000000003"), ProductoId = Guid.Parse("10000000-0000-0000-0000-000000000003"), Nombre = "Botella 2L", Sku = "GAS-COC-0003", PrecioCompraUSD = 1.60m, PrecioVentaUSD = 2.50m, Activo = true, EsBase = true, UnidadMedidaId = SeedData.UnidadBotella, CreatedAt = SeedData.Fecha, LastModifiedAt = (DateTime?)null, IsDeleted = false },
            new { Id = Guid.Parse("30000000-0000-0000-0000-000000000004"), ProductoId = Guid.Parse("10000000-0000-0000-0000-000000000004"), Nombre = "Lata 250ml", Sku = "ENE-RED-0004", PrecioCompraUSD = 1.80m, PrecioVentaUSD = 3.00m, Activo = true, EsBase = true, UnidadMedidaId = SeedData.UnidadUnidad, CreatedAt = SeedData.Fecha, LastModifiedAt = (DateTime?)null, IsDeleted = false },
            new { Id = Guid.Parse("30000000-0000-0000-0000-000000000005"), ProductoId = Guid.Parse("10000000-0000-0000-0000-000000000005"), Nombre = "Paquete 150g", Sku = "PAS-DOR-0005", PrecioCompraUSD = 1.20m, PrecioVentaUSD = 2.00m, Activo = true, EsBase = true, UnidadMedidaId = SeedData.UnidadUnidad, CreatedAt = SeedData.Fecha, LastModifiedAt = (DateTime?)null, IsDeleted = false }
        );
    }
}
