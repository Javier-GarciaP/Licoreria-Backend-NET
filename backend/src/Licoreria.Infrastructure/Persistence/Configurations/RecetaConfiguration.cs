using Licoreria.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Licoreria.Infrastructure.Persistence.Configurations;

public class RecetaConfiguration : IEntityTypeConfiguration<Receta>
{
    public void Configure(EntityTypeBuilder<Receta> builder)
    {
        builder.ToTable("recetas");
        builder.HasKey(r => r.Id);

        builder.Property(r => r.Cantidad).HasPrecision(14, 3);
        builder.HasIndex(r => new { r.ProductoId, r.VarianteInsumoId }).IsUnique();

        builder.HasOne(r => r.Producto)
               .WithMany(p => p.Recetas)
               .HasForeignKey(r => r.ProductoId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(r => r.VarianteInsumo)
               .WithMany()
               .HasForeignKey(r => r.VarianteInsumoId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
