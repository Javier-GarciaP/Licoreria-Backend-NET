using Licoreria.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Licoreria.Infrastructure.Persistence.Configurations;

public class ImpuestoConfiguration : IEntityTypeConfiguration<Impuesto>
{
    public void Configure(EntityTypeBuilder<Impuesto> builder)
    {
        builder.ToTable("impuestos");
        builder.HasKey(i => i.Id);
        builder.Property(i => i.Nombre).IsRequired().HasMaxLength(50);
        builder.Property(i => i.Porcentaje).HasPrecision(5, 2);

        builder.HasData(
            new { Id = SeedData.ImpuestoIva, Nombre = "IVA", Porcentaje = 16.00m, Activo = true, CreatedAt = SeedData.Fecha, LastModifiedAt = (DateTime?)null, IsDeleted = false },
            new { Id = SeedData.ImpuestoIgtf, Nombre = "IGTF", Porcentaje = 3.00m, Activo = true, CreatedAt = SeedData.Fecha, LastModifiedAt = (DateTime?)null, IsDeleted = false }
        );
    }
}
