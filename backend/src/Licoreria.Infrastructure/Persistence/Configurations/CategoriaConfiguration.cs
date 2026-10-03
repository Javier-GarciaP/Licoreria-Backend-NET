using Licoreria.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Licoreria.Infrastructure.Persistence.Configurations;

public class CategoriaConfiguration : IEntityTypeConfiguration<Categoria>
{
    public void Configure(EntityTypeBuilder<Categoria> builder)
    {
        builder.ToTable("categorias");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Nombre).IsRequired().HasMaxLength(50);
        builder.Property(c => c.Descripcion).HasMaxLength(200);

        builder.HasOne(c => c.CategoriaPadre)
               .WithMany(c => c.Subcategorias)
               .HasForeignKey(c => c.CategoriaPadreId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasData(
            new { Id = SeedData.CatLicores, Nombre = "Licores", Descripcion = "Rones, Whiskeys, Anises y Vodka", Activo = true, CategoriaPadreId = (Guid?)null, CreatedAt = SeedData.Fecha, LastModifiedAt = (DateTime?)null, IsDeleted = false },
            new { Id = SeedData.CatGaseosas, Nombre = "Gaseosas", Descripcion = "Refrescos y bebidas carbonatadas", Activo = true, CategoriaPadreId = (Guid?)null, CreatedAt = SeedData.Fecha, LastModifiedAt = (DateTime?)null, IsDeleted = false },
            new { Id = SeedData.CatEnergizantes, Nombre = "Energizantes", Descripcion = "Bebidas para rendimiento y energía", Activo = true, CategoriaPadreId = (Guid?)null, CreatedAt = SeedData.Fecha, LastModifiedAt = (DateTime?)null, IsDeleted = false },
            new { Id = SeedData.CatPasabocas, Nombre = "Pasabocas", Descripcion = "Snacks, papitas y frutos secos", Activo = true, CategoriaPadreId = (Guid?)null, CreatedAt = SeedData.Fecha, LastModifiedAt = (DateTime?)null, IsDeleted = false }
        );
    }
}
