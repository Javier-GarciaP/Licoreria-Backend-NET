using Licoreria.Application.Dtos;

namespace Licoreria.Application.Interfaces;

/// <summary>
/// Casos de uso del chat del personal (mesoneros, barra y cocina).
/// </summary>
public interface IServicioChat
{
    Task<IReadOnlyList<ChatMensajeDto>> ObtenerAsync(int limit = 50, CancellationToken cancellationToken = default);

    Task<ChatMensajeDto> EnviarAsync(string mensaje, CancellationToken cancellationToken = default);
}
