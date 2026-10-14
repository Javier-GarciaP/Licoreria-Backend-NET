using Licoreria.Domain.Entities;
using Licoreria.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Licoreria.Infrastructure.Persistence.Configurations;

public class TasaCambioConfiguration : IEntityTypeConfiguration<TasaCambio>
{
    public void Configure(EntityTypeBuilder<TasaCambio> builder)
    {
        builder.ToTable("tasas_cambio");
        builder.HasKey(t => t.Id);

        builder.Property(t => t.Tipo).HasConversion<string>().HasMaxLength(20);
        builder.Property(t => t.Valor).HasPrecision(18, 4);
        builder.HasIndex(t => new { t.Fecha, t.Tipo }).IsUnique();

        builder.HasData(
            new { Id = SeedData.TasaBcv, Fecha = SeedData.Fecha, Tipo = TipoTasa.BCV, Valor = 36.5000m, CreatedAt = SeedData.Fecha, LastModifiedAt = (DateTime?)null, IsDeleted = false },
            new { Id = SeedData.TasaParalelo, Fecha = SeedData.Fecha, Tipo = TipoTasa.Paralelo, Valor = 40.0000m, CreatedAt = SeedData.Fecha, LastModifiedAt = (DateTime?)null, IsDeleted = false }
        );
    }
}
