using Licoreria.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Licoreria.Infrastructure.Persistence.Configurations;

public class ListaPrecioConfiguration : IEntityTypeConfiguration<ListaPrecio>
{
    public void Configure(EntityTypeBuilder<ListaPrecio> builder)
    {
        builder.ToTable("listas_precio");
        builder.HasKey(l => l.Id);

        builder.Property(l => l.Nombre).IsRequired().HasMaxLength(50);
        builder.Property(l => l.Descripcion).HasMaxLength(200);

        builder.HasData(
            new { Id = SeedData.ListaDetal, Nombre = "Detal", Descripcion = "Precio de venta al público.", EsPredeterminada = true, Activo = true, CreatedAt = SeedData.Fecha, LastModifiedAt = (DateTime?)null, IsDeleted = false },
            new { Id = SeedData.ListaMayorista, Nombre = "Mayorista", Descripcion = "Precio para compras al mayor.", EsPredeterminada = false, Activo = true, CreatedAt = SeedData.Fecha, LastModifiedAt = (DateTime?)null, IsDeleted = false }
        );
    }
}
