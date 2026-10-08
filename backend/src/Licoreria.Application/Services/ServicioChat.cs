using Licoreria.Application.Common;
using Licoreria.Application.Dtos;
using Licoreria.Application.Interfaces;
using Licoreria.Domain.Common;
using Licoreria.Domain.Entities;

namespace Licoreria.Application.Services;

public sealed class ServicioChat : IServicioChat
{
    private const int MaximoHistorial = 100;

    private readonly IRepository<ChatMensaje> _mensajes;
    private readonly IContextoUsuario _contextoUsuario;
    private readonly INotificadorComandas _notificador;

    public ServicioChat(
        IRepository<ChatMensaje> mensajes,
        IContextoUsuario contextoUsuario,
        INotificadorComandas notificador)
    {
        _mensajes = mensajes;
        _contextoUsuario = contextoUsuario;
        _notificador = notificador;
    }

    public async Task<IReadOnlyList<ChatMensajeDto>> ObtenerAsync(int limit, CancellationToken cancellationToken = default)
    {
        var cantidad = Math.Clamp(limit, 1, MaximoHistorial);
        var mensajes = await _mensajes.FindAsync(m => !m.IsDeleted, cancellationToken);
        return mensajes
            .OrderByDescending(m => m.CreatedAt)
            .Take(cantidad)
            .OrderBy(m => m.CreatedAt)
            .Select(Mapear)
            .ToList();
    }

    public async Task<ChatMensajeDto> EnviarAsync(string mensaje, CancellationToken cancellationToken = default)
    {
        var texto = mensaje?.Trim() ?? string.Empty;
        if (texto.Length == 0)
        {
            throw new ReglaNegocioException("El mensaje no puede estar vacío.");
        }

        if (texto.Length > 500)
        {
            throw new ReglaNegocioException("El mensaje no puede superar los 500 caracteres.");
        }

        var entidad = new ChatMensaje
        {
            AutorId = _contextoUsuario.UsuarioId ?? Guid.Empty,
            AutorNombre = _contextoUsuario.Email ?? "Personal",
            Rol = _contextoUsuario.RolDominio ?? "Personal",
            Mensaje = texto
        };

        await _mensajes.AddAsync(entidad, cancellationToken);
        await _mensajes.SaveChangesAsync(cancellationToken);

        await _notificador.MensajeStaffAsync(entidad.AutorId, entidad.AutorNombre, entidad.Rol, entidad.Mensaje, cancellationToken);

        return Mapear(entidad);
    }

    private static ChatMensajeDto Mapear(ChatMensaje m)
        => new(m.Id, m.AutorId, m.AutorNombre, m.Rol, m.Mensaje, m.CreatedAt);
}
