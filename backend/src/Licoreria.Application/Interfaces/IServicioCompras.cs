using Licoreria.Application.Common;
using Licoreria.Application.Dtos;
using Licoreria.Domain.Enums;

namespace Licoreria.Application.Interfaces;

/// <summary>
/// Casos de uso de compras: proveedores y órdenes de compra.
/// </summary>
public interface IServicioCompras
{
    Task<IReadOnlyList<ProveedorDto>> ObtenerProveedoresAsync(string? busqueda = null, CancellationToken cancellationToken = default);
    Task<ProveedorDto?> ObtenerProveedorAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ProveedorDto> CrearProveedorAsync(ProveedorCrearDto dto, CancellationToken cancellationToken = default);
    Task<ProveedorDto?> EditarProveedorAsync(ProveedorEditarDto dto, CancellationToken cancellationToken = default);
    Task<bool> EliminarProveedorAsync(Guid id, CancellationToken cancellationToken = default);

    Task<ResultadoPaginado<OrdenCompraDto>> ObtenerOrdenesAsync(
        PaginacionRequest paginacion,
        EstadoOrdenCompra? estado = null,
        Guid? proveedorId = null,
        CancellationToken cancellationToken = default);

    Task<OrdenCompraDto?> ObtenerOrdenAsync(Guid id, CancellationToken cancellationToken = default);
    Task<OrdenCompraDto> CrearOrdenAsync(OrdenCompraCrearDto dto, CancellationToken cancellationToken = default);
    Task<OrdenCompraDto?> AprobarOrdenAsync(Guid id, CancellationToken cancellationToken = default);
    Task<OrdenCompraDto?> EnviarOrdenAsync(Guid id, CancellationToken cancellationToken = default);
    Task<OrdenCompraDto?> CancelarOrdenAsync(Guid id, CancellationToken cancellationToken = default);
}
