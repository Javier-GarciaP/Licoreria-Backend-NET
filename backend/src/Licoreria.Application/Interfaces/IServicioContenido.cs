using Licoreria.Application.Dtos;

namespace Licoreria.Application.Interfaces;

/// <summary>
/// Casos de uso de contenido de la web pública, menú digital y media.
/// </summary>
public interface IServicioContenido
{
    Task<IReadOnlyList<PaginaDto>> ObtenerPaginasAsync(bool soloPublicadas = false, CancellationToken cancellationToken = default);
    Task<PaginaDto?> ObtenerPaginaAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PaginaDto> CrearPaginaAsync(PaginaCrearDto dto, CancellationToken cancellationToken = default);
    Task<PaginaDto?> EditarPaginaAsync(PaginaEditarDto dto, CancellationToken cancellationToken = default);
    Task<bool> EliminarPaginaAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<HorarioDto>> ObtenerHorariosAsync(CancellationToken cancellationToken = default);
    Task<HorarioDto> GuardarHorarioAsync(HorarioGuardarDto dto, CancellationToken cancellationToken = default);

    Task<LocalInfoDto> ObtenerLocalInfoAsync(CancellationToken cancellationToken = default);
    Task<LocalInfoDto> ActualizarLocalInfoAsync(LocalInfoEditarDto dto, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<MediaAssetDto>> ObtenerMediaAsync(CancellationToken cancellationToken = default);
    Task<MediaAssetDto> RegistrarMediaAsync(string nombre, string rutaRelativa, string url, string tipo, long tamano, CancellationToken cancellationToken = default);
    Task<bool> EliminarMediaAsync(Guid id, CancellationToken cancellationToken = default);

    Task<MenuDigitalDto> ObtenerMenuDigitalAsync(CancellationToken cancellationToken = default);
    Task<QrMenuDto> ObtenerQrMenuAsync(string baseUrl, CancellationToken cancellationToken = default);
}
