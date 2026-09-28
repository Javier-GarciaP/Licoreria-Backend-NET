using Licoreria.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Licoreria.Infrastructure.Persistence.Configurations;

public class ProductoConfiguration : IEntityTypeConfiguration<Producto>
{
    public void Configure(EntityTypeBuilder<Producto> builder)
    {
        builder.ToTable("Productos");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Nombre).IsRequired().HasMaxLength(100);
        builder.Property(p => p.Sku).IsRequired().HasMaxLength(20);
        builder.HasIndex(p => p.Sku).IsUnique();
        builder.Property(p => p.CodigoBarras).HasMaxLength(50);
        builder.Property(p => p.PrecioCompraUSD).HasPrecision(18, 2);
        builder.Property(p => p.PrecioVentaUSD).HasPrecision(18, 2);
        builder.Property(p => p.StockMaximo).HasDefaultValue(0);
        builder.Property(p => p.ImagenUrl).HasMaxLength(500);

        builder.HasOne(p => p.Categoria)
               .WithMany(c => c.Productos)
               .HasForeignKey(p => p.CategoriaId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.Marca)
               .WithMany(m => m.Productos)
               .HasForeignKey(p => p.MarcaId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.UnidadMedida)
               .WithMany(u => u.Productos)
               .HasForeignKey(p => p.UnidadMedidaId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasData(
            new { Id = Guid.Parse("10000000-0000-0000-0000-000000000001"), CategoriaId = SeedData.CatLicores, MarcaId = SeedData.MarcaCacique, UnidadMedidaId = SeedData.UnidadBotella, Nombre = "Ron Cacique Añejo 0.75L", Sku = "LIC-RON-0001", CodigoBarras = "759100100101", PrecioCompraUSD = 8.50m, PrecioVentaUSD = 12.00m, Stock = 30, StockMinimo = 5, StockMaximo = 60, ImagenUrl = (string?)null, Activo = true, CreatedAt = SeedData.Fecha, LastModifiedAt = (DateTime?)null, IsDeleted = false },
            new { Id = Guid.Parse("10000000-0000-0000-0000-000000000002"), CategoriaId = SeedData.CatLicores, MarcaId = SeedData.MarcaOldParr, UnidadMedidaId = SeedData.UnidadBotella, Nombre = "Whisky Old Parr 12 Años 0.75L", Sku = "LIC-WHI-0002", CodigoBarras = "500028100202", PrecioCompraUSD = 28.00m, PrecioVentaUSD = 38.00m, Stock = 12, StockMinimo = 3, StockMaximo = 24, ImagenUrl = (string?)null, Activo = true, CreatedAt = SeedData.Fecha, LastModifiedAt = (DateTime?)null, IsDeleted = false },
            new { Id = Guid.Parse("10000000-0000-0000-0000-000000000003"), CategoriaId = SeedData.CatGaseosas, MarcaId = SeedData.MarcaCocaCola, UnidadMedidaId = SeedData.UnidadBotella, Nombre = "Coca-Cola 2 Litros", Sku = "GAS-COC-0003", CodigoBarras = "759100200303", PrecioCompraUSD = 1.60m, PrecioVentaUSD = 2.50m, Stock = 50, StockMinimo = 10, StockMaximo = 100, ImagenUrl = (string?)null, Activo = true, CreatedAt = SeedData.Fecha, LastModifiedAt = (DateTime?)null, IsDeleted = false },
            new { Id = Guid.Parse("10000000-0000-0000-0000-000000000004"), CategoriaId = SeedData.CatEnergizantes, MarcaId = SeedData.MarcaRedBull, UnidadMedidaId = SeedData.UnidadUnidad, Nombre = "Red Bull 250ml", Sku = "ENE-RED-0004", CodigoBarras = "900249010001", PrecioCompraUSD = 1.80m, PrecioVentaUSD = 3.00m, Stock = 40, StockMinimo = 8, StockMaximo = 80, ImagenUrl = (string?)null, Activo = true, CreatedAt = SeedData.Fecha, LastModifiedAt = (DateTime?)null, IsDeleted = false },
            new { Id = Guid.Parse("10000000-0000-0000-0000-000000000005"), CategoriaId = SeedData.CatPasabocas, MarcaId = SeedData.MarcaFritoLay, UnidadMedidaId = SeedData.UnidadUnidad, Nombre = "Doritos Queso Atrevido 150g", Sku = "PAS-DOR-0005", CodigoBarras = "759100400505", PrecioCompraUSD = 1.20m, PrecioVentaUSD = 2.00m, Stock = 25, StockMinimo = 5, StockMaximo = 50, ImagenUrl = (string?)null, Activo = true, CreatedAt = SeedData.Fecha, LastModifiedAt = (DateTime?)null, IsDeleted = false }
        );
    }
}
