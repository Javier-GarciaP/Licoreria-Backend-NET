using Licoreria.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Licoreria.Infrastructure.Persistence.Configurations;

public class MetodoPagoConfiguration : IEntityTypeConfiguration<MetodoPago>
{
    public void Configure(EntityTypeBuilder<MetodoPago> builder)
    {
        builder.ToTable("metodos_pago");
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Codigo).IsRequired().HasMaxLength(30);
        builder.HasIndex(m => m.Codigo).IsUnique();
        builder.Property(m => m.Nombre).IsRequired().HasMaxLength(60);

        builder.HasData(
            new { Id = SeedData.PagoEfectivoUsd, Codigo = "EfectivoUSD", Nombre = "Efectivo en dólares", Activo = true, CreatedAt = SeedData.Fecha, LastModifiedAt = (DateTime?)null, IsDeleted = false },
            new { Id = SeedData.PagoEfectivoBs, Codigo = "EfectivoBS", Nombre = "Efectivo en bolívares", Activo = true, CreatedAt = SeedData.Fecha, LastModifiedAt = (DateTime?)null, IsDeleted = false },
            new { Id = SeedData.PagoPagoMovil, Codigo = "PagoMovil", Nombre = "Pago móvil", Activo = true, CreatedAt = SeedData.Fecha, LastModifiedAt = (DateTime?)null, IsDeleted = false },
            new { Id = SeedData.PagoZelle, Codigo = "Zelle", Nombre = "Zelle", Activo = true, CreatedAt = SeedData.Fecha, LastModifiedAt = (DateTime?)null, IsDeleted = false },
            new { Id = SeedData.PagoPunto, Codigo = "Punto", Nombre = "Punto de venta", Activo = true, CreatedAt = SeedData.Fecha, LastModifiedAt = (DateTime?)null, IsDeleted = false },
            new { Id = SeedData.PagoCredito, Codigo = "Credito", Nombre = "Crédito", Activo = true, CreatedAt = SeedData.Fecha, LastModifiedAt = (DateTime?)null, IsDeleted = false }
        );
    }
}
