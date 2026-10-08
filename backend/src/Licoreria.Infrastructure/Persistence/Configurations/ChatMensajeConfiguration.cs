using Licoreria.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Licoreria.Infrastructure.Persistence.Configurations;

public class ChatMensajeConfiguration : IEntityTypeConfiguration<ChatMensaje>
{
    public void Configure(EntityTypeBuilder<ChatMensaje> builder)
    {
        builder.ToTable("chat_mensajes");
        builder.HasKey(m => m.Id);
        builder.Property(m => m.AutorNombre).HasMaxLength(120);
        builder.Property(m => m.Rol).HasMaxLength(30);
        builder.Property(m => m.Mensaje).IsRequired().HasMaxLength(500);
        builder.HasIndex(m => m.CreatedAt);
    }
}
