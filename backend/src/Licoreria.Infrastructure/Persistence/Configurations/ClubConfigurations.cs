using Licoreria.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Licoreria.Infrastructure.Persistence.Configurations;

public class ZonaConfiguration : IEntityTypeConfiguration<Zona>
{
    public void Configure(EntityTypeBuilder<Zona> builder)
    {
        builder.ToTable("zonas");
        builder.HasKey(z => z.Id);
        builder.Property(z => z.Nombre).IsRequired().HasMaxLength(60);
        builder.Property(z => z.Tipo).HasConversion<string>().HasMaxLength(20);
        builder.Property(z => z.Color).HasMaxLength(20);
        builder.Property(z => z.PosX).HasPrecision(10, 2);
        builder.Property(z => z.PosY).HasPrecision(10, 2);
        builder.Property(z => z.Ancho).HasPrecision(10, 2);
        builder.Property(z => z.Alto).HasPrecision(10, 2);
    }
}

public class MesaConfiguration : IEntityTypeConfiguration<Mesa>
{
    public void Configure(EntityTypeBuilder<Mesa> builder)
    {
        builder.ToTable("mesas");
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Numero).IsRequired().HasMaxLength(20);
        builder.Property(m => m.Forma).HasMaxLength(20);
        builder.Property(m => m.PosX).HasPrecision(10, 2);
        builder.Property(m => m.PosY).HasPrecision(10, 2);
        builder.Property(m => m.Ancho).HasPrecision(10, 2);
        builder.Property(m => m.Alto).HasPrecision(10, 2);

        builder.HasOne(m => m.Zona)
               .WithMany(z => z.Mesas)
               .HasForeignKey(m => m.ZonaId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}

public class PlanoConfiguration : IEntityTypeConfiguration<Plano>
{
    public void Configure(EntityTypeBuilder<Plano> builder)
    {
        builder.ToTable("planos");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Nombre).IsRequired().HasMaxLength(60);
        builder.Property(p => p.AnchoFondo).HasPrecision(10, 2);
        builder.Property(p => p.AltoFondo).HasPrecision(10, 2);
        builder.Property(p => p.Rejilla).HasPrecision(10, 2);
        builder.Property(p => p.Piso).HasMaxLength(20);
    }
}

public class PlanoElementoConfiguration : IEntityTypeConfiguration<PlanoElemento>
{
    public void Configure(EntityTypeBuilder<PlanoElemento> builder)
    {
        builder.ToTable("plano_elementos");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Tipo).IsRequired().HasMaxLength(30);
        builder.Property(e => e.Forma).HasMaxLength(30);
        builder.Property(e => e.Color).HasMaxLength(20);
        builder.Property(e => e.Etiqueta).HasMaxLength(60);
        builder.Property(e => e.PosX).HasPrecision(10, 2);
        builder.Property(e => e.PosY).HasPrecision(10, 2);
        builder.Property(e => e.Ancho).HasPrecision(10, 2);
        builder.Property(e => e.Alto).HasPrecision(10, 2);
        builder.Property(e => e.Rotacion).HasPrecision(10, 2);

        builder.HasOne(e => e.Plano)
               .WithMany(p => p.Elementos)
               .HasForeignKey(e => e.PlanoId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.Zona)
               .WithMany()
               .HasForeignKey(e => e.ZonaId)
               .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(e => e.Mesa)
               .WithMany()
               .HasForeignKey(e => e.MesaId)
               .OnDelete(DeleteBehavior.SetNull);
    }
}

public class ReservaConfiguration : IEntityTypeConfiguration<Reserva>
{
    public void Configure(EntityTypeBuilder<Reserva> builder)
    {
        builder.ToTable("reservas");
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Estado).HasConversion<string>().HasMaxLength(20);
        builder.Property(r => r.Origen).HasConversion<string>().HasMaxLength(20);
        builder.Property(r => r.NombreContacto).HasMaxLength(120);
        builder.Property(r => r.Telefono).HasMaxLength(30);
        builder.Property(r => r.Notas).HasMaxLength(300);
        builder.HasIndex(r => r.FechaHora);
    }
}

public class ReservaMesaConfiguration : IEntityTypeConfiguration<ReservaMesa>
{
    public void Configure(EntityTypeBuilder<ReservaMesa> builder)
    {
        builder.ToTable("reserva_mesas");
        builder.HasKey(rm => rm.Id);
        builder.HasIndex(rm => new { rm.ReservaId, rm.MesaId }).IsUnique();

        builder.HasOne(rm => rm.Reserva)
               .WithMany(r => r.Mesas)
               .HasForeignKey(rm => rm.ReservaId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(rm => rm.Mesa)
               .WithMany()
               .HasForeignKey(rm => rm.MesaId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}

public class ReservaPagoConfiguration : IEntityTypeConfiguration<ReservaPago>
{
    public void Configure(EntityTypeBuilder<ReservaPago> builder)
    {
        builder.ToTable("reserva_pagos");
        builder.HasKey(rp => rp.Id);
        builder.Property(rp => rp.Monto).HasPrecision(18, 2);
        builder.Property(rp => rp.Moneda).HasConversion<string>().HasMaxLength(10);
        builder.Property(rp => rp.Estado).HasConversion<string>().HasMaxLength(20);
        builder.Property(rp => rp.ComprobanteUrl).HasMaxLength(500);

        builder.HasOne(rp => rp.Reserva)
               .WithMany(r => r.Pagos)
               .HasForeignKey(rp => rp.ReservaId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(rp => rp.MetodoPago)
               .WithMany()
               .HasForeignKey(rp => rp.MetodoPagoId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}

public class EventoConfiguration : IEntityTypeConfiguration<Evento>
{
    public void Configure(EntityTypeBuilder<Evento> builder)
    {
        builder.ToTable("eventos");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Titulo).IsRequired().HasMaxLength(120);
        builder.Property(e => e.Descripcion).HasMaxLength(1000);
        builder.Property(e => e.ImagenUrl).HasMaxLength(500);
        builder.HasIndex(e => e.FechaInicio);
    }
}
