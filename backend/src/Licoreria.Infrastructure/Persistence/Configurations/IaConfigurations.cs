using Licoreria.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Licoreria.Infrastructure.Persistence.Configurations;

public class AiGeneracionConfiguration : IEntityTypeConfiguration<AiGeneracion>
{
    public void Configure(EntityTypeBuilder<AiGeneracion> builder)
    {
        builder.ToTable("ai_generaciones");
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Tipo).IsRequired().HasMaxLength(40);
        builder.Property(a => a.Estado).HasConversion<string>().HasMaxLength(20);
        builder.Property(a => a.Entrada).HasColumnType("jsonb");
        builder.Property(a => a.Salida).HasColumnType("jsonb");
        builder.Property(a => a.Modelo).HasMaxLength(80);
        builder.Property(a => a.Costo).HasPrecision(10, 4);
    }
}

public class PlanoGeneradoConfiguration : IEntityTypeConfiguration<PlanoGenerado>
{
    public void Configure(EntityTypeBuilder<PlanoGenerado> builder)
    {
        builder.ToTable("planos_generados");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Resultado).HasColumnType("jsonb");

        builder.HasOne(p => p.AiGeneracion)
               .WithMany()
               .HasForeignKey(p => p.AiGeneracionId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
