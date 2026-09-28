using Licoreria.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Licoreria.Infrastructure.Persistence.Configurations;

public class VentaConfiguration : IEntityTypeConfiguration<Venta>
{
    public void Configure(EntityTypeBuilder<Venta> builder)
    {
        builder.ToTable("Ventas");
        builder.HasKey(v => v.Id);
        builder.Property(v => v.TasaCambio).HasPrecision(18, 4);
        builder.Property(v => v.TotalUSD).HasPrecision(18, 2);
        builder.Property(v => v.TotalBS).HasPrecision(18, 2);
        builder.Property(v => v.MetodoPago).IsRequired().HasMaxLength(50);

        builder.HasOne(v => v.Usuario)
               .WithMany()
               .HasForeignKey(v => v.UsuarioId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
