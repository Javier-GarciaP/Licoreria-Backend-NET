using Licoreria.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Licoreria.Infrastructure.Persistence.Configurations;

public class ListaVipConfiguration : IEntityTypeConfiguration<ListaVip>
{
    public void Configure(EntityTypeBuilder<ListaVip> builder)
    {
        builder.ToTable("lista_vip");
        builder.HasKey(v => v.Id);
        builder.Property(v => v.Nombre).IsRequired().HasMaxLength(120);
        builder.Property(v => v.Documento).HasMaxLength(30);
        builder.Property(v => v.Telefono).HasMaxLength(30);
        builder.Property(v => v.Notas).HasMaxLength(300);

        builder.HasOne(v => v.Cliente)
               .WithMany()
               .HasForeignKey(v => v.ClienteId)
               .OnDelete(DeleteBehavior.SetNull);
    }
}

public class EntradaConfiguration : IEntityTypeConfiguration<Entrada>
{
    public void Configure(EntityTypeBuilder<Entrada> builder)
    {
        builder.ToTable("entradas");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Codigo).IsRequired().HasMaxLength(40);
        builder.HasIndex(e => e.Codigo).IsUnique();
        builder.Property(e => e.Precio).HasPrecision(18, 2);
        builder.Property(e => e.Moneda).HasConversion<string>().HasMaxLength(10);
        builder.Property(e => e.Estado).HasConversion<string>().HasMaxLength(20);

        builder.HasOne(e => e.Evento)
               .WithMany()
               .HasForeignKey(e => e.EventoId)
               .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(e => e.Reserva)
               .WithMany()
               .HasForeignKey(e => e.ReservaId)
               .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(e => e.Cliente)
               .WithMany()
               .HasForeignKey(e => e.ClienteId)
               .OnDelete(DeleteBehavior.SetNull);
    }
}

public class PedidoAnticipadoConfiguration : IEntityTypeConfiguration<PedidoAnticipado>
{
    public void Configure(EntityTypeBuilder<PedidoAnticipado> builder)
    {
        builder.ToTable("pedidos_anticipados");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Cantidad).HasPrecision(14, 3);
        builder.Property(p => p.PrecioUnitarioUSD).HasPrecision(18, 2);

        builder.HasOne(p => p.Reserva)
               .WithMany(r => r.Pedidos)
               .HasForeignKey(p => p.ReservaId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(p => p.Variante)
               .WithMany()
               .HasForeignKey(p => p.VarianteId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
