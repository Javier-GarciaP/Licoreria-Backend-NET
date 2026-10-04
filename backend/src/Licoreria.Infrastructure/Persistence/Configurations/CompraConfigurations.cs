using Licoreria.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Licoreria.Infrastructure.Persistence.Configurations;

public class ProveedorConfiguration : IEntityTypeConfiguration<Proveedor>
{
    public void Configure(EntityTypeBuilder<Proveedor> builder)
    {
        builder.ToTable("proveedores");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Nombre).IsRequired().HasMaxLength(120);
        builder.Property(p => p.Rif).HasMaxLength(20);
        builder.Property(p => p.Contacto).HasMaxLength(120);
        builder.Property(p => p.Telefono).HasMaxLength(30);
        builder.Property(p => p.Email).HasMaxLength(120);
        builder.Property(p => p.Direccion).HasMaxLength(300);
        builder.HasIndex(p => p.Rif);
    }
}

public class OrdenCompraConfiguration : IEntityTypeConfiguration<OrdenCompra>
{
    public void Configure(EntityTypeBuilder<OrdenCompra> builder)
    {
        builder.ToTable("ordenes_compra");
        builder.HasKey(o => o.Id);
        builder.Property(o => o.Numero).IsRequired().HasMaxLength(20);
        builder.HasIndex(o => o.Numero).IsUnique();
        builder.Property(o => o.Estado).HasConversion<string>().HasMaxLength(20);
        builder.Property(o => o.Observaciones).HasMaxLength(300);
        builder.Property(o => o.TotalUSD).HasPrecision(18, 2);
        builder.HasIndex(o => o.Fecha);

        builder.HasOne(o => o.Proveedor)
               .WithMany()
               .HasForeignKey(o => o.ProveedorId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}

public class OrdenCompraDetalleConfiguration : IEntityTypeConfiguration<OrdenCompraDetalle>
{
    public void Configure(EntityTypeBuilder<OrdenCompraDetalle> builder)
    {
        builder.ToTable("orden_compra_detalles");
        builder.HasKey(d => d.Id);
        builder.Property(d => d.Cantidad).HasPrecision(14, 3);
        builder.Property(d => d.CostoUnitarioUSD).HasPrecision(18, 2);
        builder.Property(d => d.CantidadRecibida).HasPrecision(14, 3);
        builder.Ignore(d => d.SubtotalUSD);

        builder.HasOne(d => d.OrdenCompra)
               .WithMany(o => o.Detalles)
               .HasForeignKey(d => d.OrdenCompraId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(d => d.Variante)
               .WithMany()
               .HasForeignKey(d => d.VarianteId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
