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
            new { Id = SeedData.UsuarioCajero, NombreCompleto = "Cajero Turno Mañana", Email = "cajero1@licoreria.com", PasswordHash = "100000.bGljb3JlcmlhLWNhamVybw==.vBfTiTnA2NkFbJLRMHRR/Cp00Fm6VwefpzHLwFJcVY0=", Rol = RolUsuario.Cajero, Activo = true, CreatedAt = SeedData.Fecha, LastModifiedAt = (DateTime?)null, IsDeleted = false },
            new { Id = SeedData.UsuarioMesero, NombreCompleto = "Mesero Salón", Email = "mesero1@licoreria.com", PasswordHash = "100000.AUk/0TjrGP/TROe9YBkEHg==.7/s6R8N/f1Yyq3JSRqES6ve1sn9mwRVO6ojw46ltKWs=", Rol = RolUsuario.Mesero, Activo = true, CreatedAt = SeedData.Fecha, LastModifiedAt = (DateTime?)null, IsDeleted = false },
            new { Id = SeedData.UsuarioBarra, NombreCompleto = "Barra Principal", Email = "barra1@licoreria.com", PasswordHash = "100000.yNO0fMCBb8nJcrdcPZbrXQ==.yWAcS5bMk5TWWmOyxmfMECn/wmG06aV59aV48P+UFAM=", Rol = RolUsuario.Barra, Activo = true, CreatedAt = SeedData.Fecha, LastModifiedAt = (DateTime?)null, IsDeleted = false },
            new { Id = SeedData.UsuarioCocina, NombreCompleto = "Cocina Principal", Email = "cocina1@licoreria.com", PasswordHash = "100000.tJ3DjqIW4P1+kBI6IYS3/A==./OrON+2Far9+3lLaeY7sqPpvB76AQTbBFyd33n3G9zM=", Rol = RolUsuario.Cocina, Activo = true, CreatedAt = SeedData.Fecha, LastModifiedAt = (DateTime?)null, IsDeleted = false },
            new { Id = SeedData.UsuarioHost, NombreCompleto = "Host Recepción", Email = "host1@licoreria.com", PasswordHash = "100000.QDkQ0DimXrzSqKCOZpwjZg==.5a4nNjO9hVwfdaQNMbNoS3yL781iE33Djwnq9LL5VAU=", Rol = RolUsuario.Host, Activo = true, CreatedAt = SeedData.Fecha, LastModifiedAt = (DateTime?)null, IsDeleted = false },
            new { Id = SeedData.UsuarioEditor, NombreCompleto = "Editor de Contenido", Email = "editor1@licoreria.com", PasswordHash = "100000.5IgpLdoJsYDx+QHJJZnghQ==.+FAcpwzA3baeUGj/MCPeJzFfJ3ikLCw+iw8jWGhIWH0=", Rol = RolUsuario.EditorContenido, Activo = true, CreatedAt = SeedData.Fecha, LastModifiedAt = (DateTime?)null, IsDeleted = false }
        );
    }
}
