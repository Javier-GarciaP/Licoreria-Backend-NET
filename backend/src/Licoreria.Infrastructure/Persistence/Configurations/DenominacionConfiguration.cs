using Licoreria.Domain.Entities;
using Licoreria.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Licoreria.Infrastructure.Persistence.Configurations;

public class DenominacionConfiguration : IEntityTypeConfiguration<Denominacion>
{
    public void Configure(EntityTypeBuilder<Denominacion> builder)
    {
        builder.ToTable("denominaciones");
        builder.HasKey(d => d.Id);
        builder.Property(d => d.Moneda).HasConversion<string>().HasMaxLength(10);
        builder.Property(d => d.Tipo).HasConversion<string>().HasMaxLength(10);
        builder.Property(d => d.Valor).HasPrecision(18, 2);

        var valores = new (Moneda Moneda, TipoDenominacion Tipo, decimal Valor)[]
        {
            (Moneda.USD, TipoDenominacion.Billete, 1m),
            (Moneda.USD, TipoDenominacion.Billete, 5m),
            (Moneda.USD, TipoDenominacion.Billete, 10m),
            (Moneda.USD, TipoDenominacion.Billete, 20m),
            (Moneda.USD, TipoDenominacion.Billete, 50m),
            (Moneda.USD, TipoDenominacion.Billete, 100m),
            (Moneda.BS, TipoDenominacion.Billete, 20m),
            (Moneda.BS, TipoDenominacion.Billete, 50m),
            (Moneda.BS, TipoDenominacion.Billete, 100m),
            (Moneda.BS, TipoDenominacion.Billete, 500m),
            (Moneda.BS, TipoDenominacion.Billete, 1000m)
        };

        var semilla = valores.Select((v, i) => new
        {
            Id = Guid.Parse($"cccc0000-0000-0000-0000-{i + 1:D12}"),
            v.Moneda,
            v.Tipo,
            v.Valor,
            Activo = true,
            CreatedAt = SeedData.Fecha,
            LastModifiedAt = (DateTime?)null,
            IsDeleted = false
        });

        builder.HasData(semilla);
    }
}
