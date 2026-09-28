using Licoreria.Domain.Entities;
using Licoreria.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Licoreria.Infrastructure.Persistence.Configurations;

public class UsuarioConfiguration : IEntityTypeConfiguration<Usuario>
{
    public void Configure(EntityTypeBuilder<Usuario> builder)
    {
        builder.ToTable("usuarios");
        builder.HasKey(u => u.Id);
        builder.Property(u => u.NombreCompleto).IsRequired().HasMaxLength(100);
        builder.Property(u => u.Email).IsRequired().HasMaxLength(100);
        builder.HasIndex(u => u.Email).IsUnique();
        builder.Property(u => u.Rol).HasConversion<string>().HasMaxLength(30);

        builder.HasData(
            new { Id = SeedData.UsuarioAdmin, NombreCompleto = "Administrador Principal", Email = "admin@licoreria.com", PasswordHash = "100000.bGljb3JlcmlhLWFkbWluIQ==.YwgspkL29TkDaFw34dp94bJtoYuLiheB1jTHjHoI0/A=", Rol = RolUsuario.Administrador, Activo = true, CreatedAt = SeedData.Fecha, LastModifiedAt = (DateTime?)null, IsDeleted = false },
            new { Id = SeedData.UsuarioCajero, NombreCompleto = "Cajero Turno Mañana", Email = "cajero1@licoreria.com", PasswordHash = "100000.bGljb3JlcmlhLWNhamVybw==.vBfTiTnA2NkFbJLRMHRR/Cp00Fm6VwefpzHLwFJcVY0=", Rol = RolUsuario.Cajero, Activo = true, CreatedAt = SeedData.Fecha, LastModifiedAt = (DateTime?)null, IsDeleted = false }
        );
    }
}
