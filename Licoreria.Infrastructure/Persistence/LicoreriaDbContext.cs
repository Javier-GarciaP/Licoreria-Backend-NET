using System;
using System.Collections.Generic;
using System.Text;

using Licoreria.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Licoreria.Infrastructure.Persistence;

public class LicoreriaDbContext : DbContext
{
    public LicoreriaDbContext(DbContextOptions<LicoreriaDbContext> options) : base(options)
    {
    }

    public DbSet<Categoria> Categorias => Set<Categoria>();
    public DbSet<Producto> Productos => Set<Producto>();
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Venta> Ventas => Set<Venta>();
    public DbSet<DetalleVenta> DetallesVenta => Set<DetalleVenta>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ==========================================
        // 1. FLUENT API CONFIGURATIONS
        // ==========================================

        // Categoria
        modelBuilder.Entity<Categoria>(entity =>
        {
            entity.HasKey(c => c.Id);
            entity.Property(c => c.Nombre).IsRequired().HasMaxLength(50);
            entity.Property(c => c.Descripcion).HasMaxLength(200);
        });

        // Producto
        modelBuilder.Entity<Producto>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.Property(p => p.Nombre).IsRequired().HasMaxLength(100);
            entity.Property(p => p.CodigoBarras).HasMaxLength(50);
            entity.Property(p => p.PrecioCompraUSD).HasPrecision(18, 2);
            entity.Property(p => p.PrecioVentaUSD).HasPrecision(18, 2);
            entity.Property(p => p.ImagenUrl).HasMaxLength(500);

            entity.HasOne(p => p.Categoria)
                  .WithMany(c => c.Productos)
                  .HasForeignKey(p => p.CategoriaId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Usuario
        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasKey(u => u.Id);
            entity.Property(u => u.NombreCompleto).IsRequired().HasMaxLength(100);
            entity.Property(u => u.Email).IsRequired().HasMaxLength(100);
            entity.HasIndex(u => u.Email).IsUnique();
            entity.Property(u => u.Rol).HasMaxLength(20);
        });

        // Venta
        modelBuilder.Entity<Venta>(entity =>
        {
            entity.HasKey(v => v.Id);
            entity.Property(v => v.TasaCambio).HasPrecision(18, 4);
            entity.Property(v => v.TotalUSD).HasPrecision(18, 2);
            entity.Property(v => v.TotalBS).HasPrecision(18, 2);
            entity.Property(v => v.MetodoPago).IsRequired().HasMaxLength(50);

            entity.HasOne(v => v.Usuario)
                  .WithMany()
                  .HasForeignKey(v => v.UsuarioId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // DetalleVenta
        modelBuilder.Entity<DetalleVenta>(entity =>
        {
            entity.HasKey(d => d.Id);
            entity.Property(d => d.PrecioUnitarioUSD).HasPrecision(18, 2);
            entity.Ignore(d => d.SubtotalUSD); // Propiedad calculada

            entity.HasOne(d => d.Venta)
                  .WithMany(v => v.Detalles)
                  .HasForeignKey(d => d.VentaId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(d => d.Producto)
                  .WithMany()
                  .HasForeignKey(d => d.ProductoId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // ==========================================
        // 2. DATA SEEDING (DATOS INICIALES DE PRUEBA)
        // ==========================================
        // Se utilizan objetos anónimos con identificadores Guid fijos
        // para que las migraciones sean deterministas (requerido por HasData).

        var seedDate = new DateTime(2026, 9, 23, 0, 0, 0, DateTimeKind.Utc);

        // --- Categorías ---
        var catLicores = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var catGaseosas = Guid.Parse("22222222-2222-2222-2222-222222222222");
        var catEnergizantes = Guid.Parse("33333333-3333-3333-3333-333333333333");
        var catPasabocas = Guid.Parse("44444444-4444-4444-4444-444444444444");

        modelBuilder.Entity<Categoria>().HasData(
            new { Id = catLicores, Nombre = "Licores", Descripcion = "Rones, Whiskeys, Anises y Vodka", CreatedAt = seedDate, LastModifiedAt = (DateTime?)null, IsDeleted = false },
            new { Id = catGaseosas, Nombre = "Gaseosas", Descripcion = "Refrescos y bebidas carbonatadas", CreatedAt = seedDate, LastModifiedAt = (DateTime?)null, IsDeleted = false },
            new { Id = catEnergizantes, Nombre = "Energizantes", Descripcion = "Bebidas para rendimiento y energía", CreatedAt = seedDate, LastModifiedAt = (DateTime?)null, IsDeleted = false },
            new { Id = catPasabocas, Nombre = "Pasabocas", Descripcion = "Snacks, papitas y frutos secos", CreatedAt = seedDate, LastModifiedAt = (DateTime?)null, IsDeleted = false }
        );

        // --- Productos ---
        modelBuilder.Entity<Producto>().HasData(
            new { Id = Guid.Parse("10000000-0000-0000-0000-000000000001"), CategoriaId = catLicores, Nombre = "Ron Cacique Añejo 0.75L", CodigoBarras = "759100100101", PrecioCompraUSD = 8.50m, PrecioVentaUSD = 12.00m, Stock = 30, StockMinimo = 5, ImagenUrl = (string?)null, Activo = true, CreatedAt = seedDate, LastModifiedAt = (DateTime?)null, IsDeleted = false },
            new { Id = Guid.Parse("10000000-0000-0000-0000-000000000002"), CategoriaId = catLicores, Nombre = "Whisky Old Parr 12 Años 0.75L", CodigoBarras = "500028100202", PrecioCompraUSD = 28.00m, PrecioVentaUSD = 38.00m, Stock = 12, StockMinimo = 3, ImagenUrl = (string?)null, Activo = true, CreatedAt = seedDate, LastModifiedAt = (DateTime?)null, IsDeleted = false },
            new { Id = Guid.Parse("10000000-0000-0000-0000-000000000003"), CategoriaId = catGaseosas, Nombre = "Coca-Cola 2 Litros", CodigoBarras = "759100200303", PrecioCompraUSD = 1.60m, PrecioVentaUSD = 2.50m, Stock = 50, StockMinimo = 10, ImagenUrl = (string?)null, Activo = true, CreatedAt = seedDate, LastModifiedAt = (DateTime?)null, IsDeleted = false },
            new { Id = Guid.Parse("10000000-0000-0000-0000-000000000004"), CategoriaId = catEnergizantes, Nombre = "Red Bull 250ml", CodigoBarras = "900249010001", PrecioCompraUSD = 1.80m, PrecioVentaUSD = 3.00m, Stock = 40, StockMinimo = 8, ImagenUrl = (string?)null, Activo = true, CreatedAt = seedDate, LastModifiedAt = (DateTime?)null, IsDeleted = false },
            new { Id = Guid.Parse("10000000-0000-0000-0000-000000000005"), CategoriaId = catPasabocas, Nombre = "Doritos Queso Atrevido 150g", CodigoBarras = "759100400505", PrecioCompraUSD = 1.20m, PrecioVentaUSD = 2.00m, Stock = 25, StockMinimo = 5, ImagenUrl = (string?)null, Activo = true, CreatedAt = seedDate, LastModifiedAt = (DateTime?)null, IsDeleted = false }
        );

        // --- Usuarios ---
        modelBuilder.Entity<Usuario>().HasData(
            new { Id = Guid.Parse("20000000-0000-0000-0000-000000000001"), NombreCompleto = "Administrador Principal", Email = "admin@licoreria.com", PasswordHash = "admin123_hash", Rol = "Admin", Activo = true, CreatedAt = seedDate, LastModifiedAt = (DateTime?)null, IsDeleted = false },
            new { Id = Guid.Parse("20000000-0000-0000-0000-000000000002"), NombreCompleto = "Cajero Turno Mañana", Email = "cajero1@licoreria.com", PasswordHash = "cajero123_hash", Rol = "Cajero", Activo = true, CreatedAt = seedDate, LastModifiedAt = (DateTime?)null, IsDeleted = false }
        );
    }
}
