using Licoreria.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Licoreria.Infrastructure.Persistence.Configurations;

public class CuentaDivisionConfiguration : IEntityTypeConfiguration<CuentaDivision>
{
    public void Configure(EntityTypeBuilder<CuentaDivision> builder)
    {
        builder.ToTable("cuenta_divisiones");
        builder.HasKey(d => d.Id);
        builder.Property(d => d.Monto).HasPrecision(18, 2);

        builder.HasOne(d => d.Cuenta)
               .WithMany(c => c.Divisiones)
               .HasForeignKey(d => d.CuentaId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}

public class PromocionConfiguration : IEntityTypeConfiguration<Promocion>
{
    public void Configure(EntityTypeBuilder<Promocion> builder)
    {
        builder.ToTable("promociones");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Nombre).IsRequired().HasMaxLength(120);
        builder.Property(p => p.Tipo).HasConversion<string>().HasMaxLength(20);
        builder.Property(p => p.Valor).HasPrecision(18, 2);
    }
}
