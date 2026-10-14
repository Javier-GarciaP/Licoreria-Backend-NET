using Licoreria.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Licoreria.Infrastructure.Persistence.Configurations;

public class CodigoBarrasConfiguration : IEntityTypeConfiguration<CodigoBarras>
{
    public void Configure(EntityTypeBuilder<CodigoBarras> builder)
    {
        builder.ToTable("codigos_barras");
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Codigo).IsRequired().HasMaxLength(50);
        builder.HasIndex(c => c.Codigo).IsUnique();

        builder.HasOne(c => c.Variante)
               .WithMany(v => v.CodigosBarras)
               .HasForeignKey(c => c.VarianteId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasData(
            new { Id = Guid.Parse("40000000-0000-0000-0000-000000000001"), VarianteId = Guid.Parse("30000000-0000-0000-0000-000000000001"), Codigo = "759100100101", Principal = true, CreatedAt = SeedData.Fecha, LastModifiedAt = (DateTime?)null, IsDeleted = false },
            new { Id = Guid.Parse("40000000-0000-0000-0000-000000000002"), VarianteId = Guid.Parse("30000000-0000-0000-0000-000000000002"), Codigo = "500028100202", Principal = true, CreatedAt = SeedData.Fecha, LastModifiedAt = (DateTime?)null, IsDeleted = false },
            new { Id = Guid.Parse("40000000-0000-0000-0000-000000000003"), VarianteId = Guid.Parse("30000000-0000-0000-0000-000000000003"), Codigo = "759100200303", Principal = true, CreatedAt = SeedData.Fecha, LastModifiedAt = (DateTime?)null, IsDeleted = false },
            new { Id = Guid.Parse("40000000-0000-0000-0000-000000000004"), VarianteId = Guid.Parse("30000000-0000-0000-0000-000000000004"), Codigo = "900249010001", Principal = true, CreatedAt = SeedData.Fecha, LastModifiedAt = (DateTime?)null, IsDeleted = false },
            new { Id = Guid.Parse("40000000-0000-0000-0000-000000000005"), VarianteId = Guid.Parse("30000000-0000-0000-0000-000000000005"), Codigo = "759100400505", Principal = true, CreatedAt = SeedData.Fecha, LastModifiedAt = (DateTime?)null, IsDeleted = false }
        );
    }
}
