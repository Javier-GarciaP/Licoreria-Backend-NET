using Licoreria.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Licoreria.Infrastructure.Persistence.Configurations;

public class ComprobanteFiscalConfiguration : IEntityTypeConfiguration<ComprobanteFiscal>
{
    public void Configure(EntityTypeBuilder<ComprobanteFiscal> builder)
    {
        builder.ToTable("comprobantes_fiscales");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Numero).IsRequired().HasMaxLength(20);
        builder.Property(c => c.NumeroControl).IsRequired().HasMaxLength(20);
        builder.Property(c => c.TotalUSD).HasPrecision(18, 2);
        builder.Property(c => c.TotalBS).HasPrecision(18, 2);
        builder.HasIndex(c => c.VentaId).IsUnique();

        builder.HasOne(c => c.Venta)
               .WithOne(v => v.Comprobante)
               .HasForeignKey<ComprobanteFiscal>(c => c.VentaId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
