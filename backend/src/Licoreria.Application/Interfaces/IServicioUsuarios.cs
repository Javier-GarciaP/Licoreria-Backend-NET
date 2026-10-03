using Licoreria.Application.Common;
using Licoreria.Application.Dtos;

namespace Licoreria.Application.Interfaces;

/// <summary>
/// Casos de uso de administración de usuarios del sistema.
/// </summary>
public interface IServicioUsuarios
{
    Task<ResultadoPaginado<UsuarioDto>> ObtenerUsuariosAsync(
        PaginacionRequest paginacion,
        string? busqueda = null,
        CancellationToken cancellationToken = default);

    Task<UsuarioDto?> ObtenerUsuarioAsync(Guid id, CancellationToken cancellationToken = default);

    Task<UsuarioDto> CrearUsuarioAsync(UsuarioCrearDto dto, CancellationToken cancellationToken = default);

    Task<UsuarioDto?> EditarUsuarioAsync(UsuarioEditarDto dto, CancellationToken cancellationToken = default);

    Task<bool> EliminarUsuarioAsync(Guid id, CancellationToken cancellationToken = default);

    Task<bool> CambiarPasswordAsync(Guid id, CambiarPasswordDto dto, CancellationToken cancellationToken = default);
}
