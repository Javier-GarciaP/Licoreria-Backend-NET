using Licoreria.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Licoreria.Infrastructure.Persistence.Configurations;

public class MermaConfiguration : IEntityTypeConfiguration<Merma>
{
    public void Configure(EntityTypeBuilder<Merma> builder)
    {
        builder.ToTable("mermas");
        builder.HasKey(m => m.Id);

        builder.Property(m => m.Motivo).HasConversion<string>().HasMaxLength(20);
        builder.HasIndex(m => m.MovimientoId).IsUnique();

        builder.HasOne(m => m.Movimiento)
               .WithMany()
               .HasForeignKey(m => m.MovimientoId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
