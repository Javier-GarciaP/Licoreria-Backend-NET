using Licoreria.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Licoreria.Infrastructure.Persistence.Configurations;

public class ModificadorConfiguration : IEntityTypeConfiguration<Modificador>
{
    public void Configure(EntityTypeBuilder<Modificador> builder)
    {
        builder.ToTable("modificadores");
        builder.HasKey(m => m.Id);

        builder.Property(m => m.Nombre).IsRequired().HasMaxLength(100);
        builder.Property(m => m.PrecioAdicional).HasPrecision(18, 2);
    }
}

public class ProductoModificadorConfiguration : IEntityTypeConfiguration<ProductoModificador>
{
    public void Configure(EntityTypeBuilder<ProductoModificador> builder)
    {
        builder.ToTable("producto_modificadores");
        builder.HasKey(pm => pm.Id);
        builder.HasIndex(pm => new { pm.ProductoId, pm.ModificadorId }).IsUnique();

        builder.HasOne(pm => pm.Producto)
               .WithMany(p => p.Modificadores)
               .HasForeignKey(pm => pm.ProductoId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(pm => pm.Modificador)
               .WithMany(m => m.Productos)
               .HasForeignKey(pm => pm.ModificadorId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
