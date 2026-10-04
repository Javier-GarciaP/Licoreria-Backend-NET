using Licoreria.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Licoreria.Infrastructure.Persistence.Configurations;

public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("audit_log");
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Usuario).HasMaxLength(120);
        builder.Property(a => a.Accion).IsRequired().HasMaxLength(80);
        builder.Property(a => a.Entidad).IsRequired().HasMaxLength(60);
        builder.Property(a => a.Datos).HasColumnType("jsonb");
        builder.Property(a => a.Ip).HasMaxLength(60);
        builder.HasIndex(a => a.CreatedAt);
        builder.HasIndex(a => a.Entidad);
    }
}
