using Licoreria.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Licoreria.Infrastructure.Persistence.Configurations;

public class PaginaConfiguration : IEntityTypeConfiguration<Pagina>
{
    public void Configure(EntityTypeBuilder<Pagina> builder)
    {
        builder.ToTable("paginas");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Titulo).IsRequired().HasMaxLength(120);
        builder.Property(p => p.Slug).IsRequired().HasMaxLength(120);
        builder.HasIndex(p => p.Slug).IsUnique();
    }
}

public class SeccionConfiguration : IEntityTypeConfiguration<Seccion>
{
    public void Configure(EntityTypeBuilder<Seccion> builder)
    {
        builder.ToTable("secciones");
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Titulo).IsRequired().HasMaxLength(120);
        builder.Property(s => s.Tipo).HasMaxLength(40);

        builder.HasOne(s => s.Pagina)
               .WithMany(p => p.Secciones)
               .HasForeignKey(s => s.PaginaId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}

public class BloqueContenidoConfiguration : IEntityTypeConfiguration<BloqueContenido>
{
    public void Configure(EntityTypeBuilder<BloqueContenido> builder)
    {
        builder.ToTable("bloques_contenido");
        builder.HasKey(b => b.Id);
        builder.Property(b => b.Tipo).HasMaxLength(40);
        builder.Property(b => b.Contenido).HasColumnType("jsonb");

        builder.HasOne(b => b.Seccion)
               .WithMany(s => s.Bloques)
               .HasForeignKey(b => b.SeccionId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}

public class MediaAssetConfiguration : IEntityTypeConfiguration<MediaAsset>
{
    public void Configure(EntityTypeBuilder<MediaAsset> builder)
    {
        builder.ToTable("media_assets");
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Nombre).IsRequired().HasMaxLength(200);
        builder.Property(m => m.RutaRelativa).IsRequired().HasMaxLength(500);
        builder.Property(m => m.Url).IsRequired().HasMaxLength(500);
        builder.Property(m => m.Tipo).HasMaxLength(40);
    }
}

public class HorarioAtencionConfiguration : IEntityTypeConfiguration<HorarioAtencion>
{
    public void Configure(EntityTypeBuilder<HorarioAtencion> builder)
    {
        builder.ToTable("horarios_atencion");
        builder.HasKey(h => h.Id);
    }
}

public class LocalInfoConfiguration : IEntityTypeConfiguration<LocalInfo>
{
    public void Configure(EntityTypeBuilder<LocalInfo> builder)
    {
        builder.ToTable("local_info");
        builder.HasKey(l => l.Id);
        builder.Property(l => l.Nombre).IsRequired().HasMaxLength(120);
        builder.Property(l => l.Descripcion).HasMaxLength(1000);
        builder.Property(l => l.Direccion).HasMaxLength(300);
        builder.Property(l => l.Telefono).HasMaxLength(30);
        builder.Property(l => l.Whatsapp).HasMaxLength(30);
        builder.Property(l => l.Email).HasMaxLength(120);
        builder.Property(l => l.Instagram).HasMaxLength(200);
        builder.Property(l => l.Facebook).HasMaxLength(200);
        builder.Property(l => l.MapaUrl).HasMaxLength(500);
        builder.Property(l => l.LogoUrl).HasMaxLength(500);
    }
}
