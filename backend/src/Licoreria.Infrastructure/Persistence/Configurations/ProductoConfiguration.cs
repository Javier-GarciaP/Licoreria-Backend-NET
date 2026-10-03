using Licoreria.Domain.Entities;
using Licoreria.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Licoreria.Infrastructure.Persistence.Configurations;

public class ProductoConfiguration : IEntityTypeConfiguration<Producto>
{
    public void Configure(EntityTypeBuilder<Producto> builder)
    {
        builder.ToTable("productos");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Nombre).IsRequired().HasMaxLength(120);
        builder.Property(p => p.Descripcion).HasMaxLength(300);
        builder.Property(p => p.ImagenUrl).HasMaxLength(500);
        builder.Property(p => p.Tipo).HasConversion<string>().HasMaxLength(20);
        builder.Property(p => p.GradoAlcoholico).HasPrecision(5, 2);

        builder.HasOne(p => p.Categoria)
               .WithMany(c => c.Productos)
               .HasForeignKey(p => p.CategoriaId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.Marca)
               .WithMany(m => m.Productos)
               .HasForeignKey(p => p.MarcaId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.Impuesto)
               .WithMany(i => i.Productos)
               .HasForeignKey(p => p.ImpuestoId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasData(
            new { Id = Guid.Parse("10000000-0000-0000-0000-000000000001"), Nombre = "Ron Cacique Añejo", Descripcion = "Ron añejo venezolano.", ImagenUrl = (string?)null, Tipo = TipoProducto.Simple, GradoAlcoholico = (decimal?)40.00m, Activo = true, CategoriaId = SeedData.CatLicores, MarcaId = (Guid?)SeedData.MarcaCacique, ImpuestoId = (Guid?)SeedData.ImpuestoIva, CreatedAt = SeedData.Fecha, LastModifiedAt = (DateTime?)null, IsDeleted = false },
            new { Id = Guid.Parse("10000000-0000-0000-0000-000000000002"), Nombre = "Whisky Old Parr 12 Años", Descripcion = "Whisky escocés de 12 años.", ImagenUrl = (string?)null, Tipo = TipoProducto.Simple, GradoAlcoholico = (decimal?)40.00m, Activo = true, CategoriaId = SeedData.CatLicores, MarcaId = (Guid?)SeedData.MarcaOldParr, ImpuestoId = (Guid?)SeedData.ImpuestoIva, CreatedAt = SeedData.Fecha, LastModifiedAt = (DateTime?)null, IsDeleted = false },
            new { Id = Guid.Parse("10000000-0000-0000-0000-000000000003"), Nombre = "Coca-Cola", Descripcion = "Refresco de cola.", ImagenUrl = (string?)null, Tipo = TipoProducto.Simple, GradoAlcoholico = (decimal?)null, Activo = true, CategoriaId = SeedData.CatGaseosas, MarcaId = (Guid?)SeedData.MarcaCocaCola, ImpuestoId = (Guid?)SeedData.ImpuestoIva, CreatedAt = SeedData.Fecha, LastModifiedAt = (DateTime?)null, IsDeleted = false },
            new { Id = Guid.Parse("10000000-0000-0000-0000-000000000004"), Nombre = "Red Bull", Descripcion = "Bebida energizante.", ImagenUrl = (string?)null, Tipo = TipoProducto.Simple, GradoAlcoholico = (decimal?)null, Activo = true, CategoriaId = SeedData.CatEnergizantes, MarcaId = (Guid?)SeedData.MarcaRedBull, ImpuestoId = (Guid?)SeedData.ImpuestoIva, CreatedAt = SeedData.Fecha, LastModifiedAt = (DateTime?)null, IsDeleted = false },
            new { Id = Guid.Parse("10000000-0000-0000-0000-000000000005"), Nombre = "Doritos Queso Atrevido", Descripcion = "Snack de maíz sabor queso.", ImagenUrl = (string?)null, Tipo = TipoProducto.Simple, GradoAlcoholico = (decimal?)null, Activo = true, CategoriaId = SeedData.CatPasabocas, MarcaId = (Guid?)SeedData.MarcaFritoLay, ImpuestoId = (Guid?)SeedData.ImpuestoIva, CreatedAt = SeedData.Fecha, LastModifiedAt = (DateTime?)null, IsDeleted = false }
        );
    }
}
