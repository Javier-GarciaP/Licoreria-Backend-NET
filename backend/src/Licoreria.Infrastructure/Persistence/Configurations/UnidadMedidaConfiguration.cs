using Licoreria.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Licoreria.Infrastructure.Persistence.Configurations;

public class UnidadMedidaConfiguration : IEntityTypeConfiguration<UnidadMedida>
{
    public void Configure(EntityTypeBuilder<UnidadMedida> builder)
    {
        builder.ToTable("unidades_medida");
        builder.HasKey(u => u.Id);
        builder.Property(u => u.Nombre).IsRequired().HasMaxLength(50);
        builder.Property(u => u.Abreviatura).IsRequired().HasMaxLength(10);

        builder.HasData(
            new { Id = SeedData.UnidadBotella, Nombre = "Botella", Abreviatura = "BOT", CreatedAt = SeedData.Fecha, LastModifiedAt = (DateTime?)null, IsDeleted = false },
            new { Id = SeedData.UnidadUnidad, Nombre = "Unidad", Abreviatura = "UND", CreatedAt = SeedData.Fecha, LastModifiedAt = (DateTime?)null, IsDeleted = false },
            new { Id = SeedData.UnidadTobo, Nombre = "Tobo", Abreviatura = "TOB", CreatedAt = SeedData.Fecha, LastModifiedAt = (DateTime?)null, IsDeleted = false },
            new { Id = SeedData.UnidadPlato, Nombre = "Plato", Abreviatura = "PLA", CreatedAt = SeedData.Fecha, LastModifiedAt = (DateTime?)null, IsDeleted = false },
            new { Id = SeedData.UnidadTrago, Nombre = "Trago", Abreviatura = "TRA", CreatedAt = SeedData.Fecha, LastModifiedAt = (DateTime?)null, IsDeleted = false }
        );
    }
}
