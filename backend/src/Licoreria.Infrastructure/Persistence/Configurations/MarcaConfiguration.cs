using Licoreria.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Licoreria.Infrastructure.Persistence.Configurations;

public class MarcaConfiguration : IEntityTypeConfiguration<Marca>
{
    public void Configure(EntityTypeBuilder<Marca> builder)
    {
        builder.ToTable("Marcas");
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Nombre).IsRequired().HasMaxLength(100);
        builder.Property(m => m.Descripcion).HasMaxLength(200);

        builder.HasData(
            new { Id = SeedData.MarcaCacique, Nombre = "Cacique", Descripcion = "Ron venezolano", Activo = true, CreatedAt = SeedData.Fecha, LastModifiedAt = (DateTime?)null, IsDeleted = false },
            new { Id = SeedData.MarcaOldParr, Nombre = "Old Parr", Descripcion = "Whisky escocés", Activo = true, CreatedAt = SeedData.Fecha, LastModifiedAt = (DateTime?)null, IsDeleted = false },
            new { Id = SeedData.MarcaCocaCola, Nombre = "Coca-Cola", Descripcion = "Bebidas gaseosas", Activo = true, CreatedAt = SeedData.Fecha, LastModifiedAt = (DateTime?)null, IsDeleted = false },
            new { Id = SeedData.MarcaRedBull, Nombre = "Red Bull", Descripcion = "Bebidas energizantes", Activo = true, CreatedAt = SeedData.Fecha, LastModifiedAt = (DateTime?)null, IsDeleted = false },
            new { Id = SeedData.MarcaFritoLay, Nombre = "Frito-Lay", Descripcion = "Snacks y pasabocas", Activo = true, CreatedAt = SeedData.Fecha, LastModifiedAt = (DateTime?)null, IsDeleted = false }
        );
    }
}
