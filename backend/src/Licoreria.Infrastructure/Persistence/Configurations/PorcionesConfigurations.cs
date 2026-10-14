using Licoreria.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Licoreria.Infrastructure.Persistence.Configurations;

public class PorcionOrigenConfiguration : IEntityTypeConfiguration<PorcionOrigen>
{
    public void Configure(EntityTypeBuilder<PorcionOrigen> builder)
    {
        builder.ToTable("porcion_origenes", tabla =>
            tabla.HasCheckConstraint("ck_porcion_origen_porciones_por_unidad", "\"PorcionesPorUnidad\" >= 1"));
        builder.HasKey(o => o.Id);

        builder.HasIndex(o => new { o.PorcionVarianteId, o.PresentacionVarianteId })
               .IsUnique()
               .HasDatabaseName("ux_porcion_origen_par");

        builder.HasOne(o => o.PorcionVariante)
               .WithMany(v => v.Origenes)
               .HasForeignKey(o => o.PorcionVarianteId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(o => o.PresentacionVariante)
               .WithMany()
               .HasForeignKey(o => o.PresentacionVarianteId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}

public class EnvaseAbiertoConfiguration : IEntityTypeConfiguration<EnvaseAbierto>
{
    public void Configure(EntityTypeBuilder<EnvaseAbierto> builder)
    {
        builder.ToTable("envases_abiertos", tabla =>
        {
            tabla.HasCheckConstraint("ck_envase_restantes_no_negativas", "\"PorcionesRestantes\" >= 0");
            tabla.HasCheckConstraint("ck_envase_restantes_no_superan_iniciales", "\"PorcionesRestantes\" <= \"PorcionesIniciales\"");
            tabla.HasCheckConstraint("ck_envase_iniciales_positivas", "\"PorcionesIniciales\" >= 1");
        });
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Estado).HasConversion<string>().HasMaxLength(20);
        builder.Property(e => e.MotivoDescarte).HasMaxLength(200);
        builder.HasIndex(e => new { e.PorcionVarianteId, e.Estado, e.AbiertoEn })
               .HasDatabaseName("ix_envases_porcion_estado_fecha");

        builder.HasOne(e => e.PorcionVariante)
               .WithMany()
               .HasForeignKey(e => e.PorcionVarianteId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.PresentacionVariante)
               .WithMany()
               .HasForeignKey(e => e.PresentacionVarianteId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}

public class StockPorcionConfiguration : IEntityTypeConfiguration<StockPorcion>
{
    public void Configure(EntityTypeBuilder<StockPorcion> builder)
    {
        builder.ToTable("stock_porcion", tabla =>
            tabla.HasCheckConstraint("ck_stock_porcion_reservadas_no_negativas", "\"PorcionesReservadas\" >= 0"));
        builder.HasKey(s => s.Id);
        builder.HasIndex(s => s.VarianteId).IsUnique().HasDatabaseName("ux_stock_porcion_variante");

        builder.HasOne(s => s.Variante)
               .WithMany()
               .HasForeignKey(s => s.VarianteId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
