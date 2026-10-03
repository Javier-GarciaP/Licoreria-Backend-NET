using Licoreria.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Licoreria.Infrastructure.Persistence.Configurations;

public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("refresh_tokens");
        builder.HasKey(t => t.Id);

        builder.Property(t => t.Token).IsRequired().HasMaxLength(200);
        builder.HasIndex(t => t.Token).IsUnique();

        builder.Property(t => t.Revocado).IsRequired();
        builder.Property(t => t.ExpiraEn).IsRequired();

        builder.HasOne(t => t.Usuario)
               .WithMany()
               .HasForeignKey(t => t.UsuarioId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(t => t.UsuarioId);
    }
}
