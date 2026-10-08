using Licoreria.Application.Common;
using Licoreria.Domain.Entities;
using Licoreria.Domain.Enums;

namespace Licoreria.Application.Interfaces;

public interface ICuentaRepository
{
    Task<ResultadoPaginado<Cuenta>> ObtenerPaginadoAsync(
        PaginacionRequest paginacion,
        EstadoCuenta? estado = null,
        Guid? usuarioId = null,
        CancellationToken cancellationToken = default);

    Task<Cuenta?> ObtenerConDetalleAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>Cuenta abierta (sesión de mesa sin cerrar) asociada a una mesa, si existe.</summary>
    Task<Cuenta?> ObtenerCuentaAbiertaPorMesaAsync(Guid mesaId, CancellationToken cancellationToken = default);

    Task AgregarAsync(Cuenta cuenta, CancellationToken cancellationToken = default);

    Task AgregarComandaAsync(Comanda comanda, CancellationToken cancellationToken = default);

    Task AgregarAbonoAsync(Abono abono, CancellationToken cancellationToken = default);

    Task<ComandaDetalle?> ObtenerDetalleAsync(Guid comandaId, Guid detalleId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Guid>> ObtenerMesasOcupadasAsync(CancellationToken cancellationToken = default);

    /// <summary>Mapa mesa → cuenta abierta (sesión de mesa sin cerrar), si existe.</summary>
    Task<IReadOnlyDictionary<Guid, Guid>> ObtenerCuentasAbiertasPorMesaAsync(CancellationToken cancellationToken = default);

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
