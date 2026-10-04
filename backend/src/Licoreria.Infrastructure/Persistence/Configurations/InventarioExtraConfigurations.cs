using Licoreria.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Licoreria.Infrastructure.Persistence.Configurations;

public class LoteConfiguration : IEntityTypeConfiguration<Lote>
{
    public void Configure(EntityTypeBuilder<Lote> builder)
    {
        builder.ToTable("lotes");
        builder.HasKey(l => l.Id);
        builder.Property(l => l.Codigo).IsRequired().HasMaxLength(50);
        builder.Property(l => l.Cantidad).HasPrecision(14, 3);
        builder.HasIndex(l => l.FechaVencimiento);

        builder.HasOne(l => l.Variante)
               .WithMany()
               .HasForeignKey(l => l.VarianteId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}

public class TomaFisicaConfiguration : IEntityTypeConfiguration<TomaFisica>
{
    public void Configure(EntityTypeBuilder<TomaFisica> builder)
    {
        builder.ToTable("tomas_fisicas");
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Estado).HasConversion<string>().HasMaxLength(20);
        builder.Property(t => t.Observaciones).HasMaxLength(300);

        builder.HasOne(t => t.Usuario)
               .WithMany()
               .HasForeignKey(t => t.UsuarioId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}

public class TomaFisicaDetalleConfiguration : IEntityTypeConfiguration<TomaFisicaDetalle>
{
    public void Configure(EntityTypeBuilder<TomaFisicaDetalle> builder)
    {
        builder.ToTable("toma_fisica_detalles");
        builder.HasKey(d => d.Id);
        builder.Property(d => d.CantidadSistema).HasPrecision(14, 3);
        builder.Property(d => d.CantidadContada).HasPrecision(14, 3);
        builder.Ignore(d => d.Diferencia);

        builder.HasOne(d => d.TomaFisica)
               .WithMany(t => t.Detalles)
               .HasForeignKey(d => d.TomaFisicaId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(d => d.Variante)
               .WithMany()
               .HasForeignKey(d => d.VarianteId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
