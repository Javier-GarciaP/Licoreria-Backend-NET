using Licoreria.Application.Common;
using Licoreria.Application.Dtos;
using Licoreria.Application.Interfaces;
using Licoreria.Domain.Common;
using Licoreria.Domain.Entities;
using Licoreria.Domain.Enums;

namespace Licoreria.Application.Services;

public sealed class ServicioCaja : IServicioCaja
{
    private readonly ISesionCajaRepository _sesionRepository;
    private readonly IRepository<Denominacion> _denominacionRepository;
    private readonly IContextoUsuario _contextoUsuario;
    private readonly IRelojSistema _reloj;

    public ServicioCaja(
        ISesionCajaRepository sesionRepository,
        IRepository<Denominacion> denominacionRepository,
        IContextoUsuario contextoUsuario,
        IRelojSistema reloj)
    {
        _sesionRepository = sesionRepository;
        _denominacionRepository = denominacionRepository;
        _contextoUsuario = contextoUsuario;
        _reloj = reloj;
    }

    public async Task<IReadOnlyList<DenominacionDto>> ObtenerDenominacionesAsync(CancellationToken cancellationToken = default)
    {
        var denominaciones = await _denominacionRepository.GetAllAsync(cancellationToken);
        return denominaciones
            .Where(d => !d.IsDeleted && d.Activo)
            .OrderBy(d => d.Moneda).ThenByDescending(d => d.Valor)
            .Select(d => new DenominacionDto(d.Id, d.Moneda, d.Tipo, d.Valor))
            .ToList();
    }

    public async Task<SesionCajaDto> AbrirSesionAsync(AbrirCajaDto dto, CancellationToken cancellationToken = default)
    {
        var abierta = await _sesionRepository.ObtenerAbiertaAsync(cancellationToken);
        if (abierta is not null)
        {
            throw new ConflictoException("Ya existe una sesión de caja abierta.");
        }

        if (dto.FondoInicial < 0)
        {
            throw new ReglaNegocioException("El fondo inicial no puede ser negativo.");
        }

        var usuarioId = _contextoUsuario.UsuarioId
            ?? throw new ReglaNegocioException("No se pudo identificar al usuario que abre la caja.");

        var sesion = new SesionCaja
        {
            UsuarioId = usuarioId,
            FondoInicial = dto.FondoInicial,
            AbiertaEn = _reloj.UtcNow
        };

        await _sesionRepository.AgregarAsync(sesion, cancellationToken);
        await _sesionRepository.SaveChangesAsync(cancellationToken);

        return Mapear(sesion);
    }

    public async Task<SesionCajaDto?> ObtenerSesionActivaAsync(CancellationToken cancellationToken = default)
    {
        var sesion = await _sesionRepository.ObtenerAbiertaAsync(cancellationToken);
        return sesion is null ? null : Mapear(sesion);
    }

    public async Task<SesionCajaDto?> ObtenerSesionAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var sesion = await _sesionRepository.ObtenerConDetalleAsync(id, cancellationToken);
        return sesion is null ? null : Mapear(sesion);
    }

    public async Task<ResultadoPaginado<SesionCajaDto>> ObtenerSesionesAsync(
        PaginacionRequest paginacion,
        CancellationToken cancellationToken = default)
    {
        var pagina = await _sesionRepository.ObtenerPaginadoAsync(paginacion, cancellationToken);
        var items = pagina.Items.Select(Mapear).ToList();
        return ResultadoPaginado<SesionCajaDto>.Crear(items, pagina.Page, pagina.PageSize, pagina.TotalItems);
    }

    public async Task<SesionCajaDto?> RegistrarMovimientoAsync(
        Guid sesionId,
        MovimientoCajaCrearDto dto,
        CancellationToken cancellationToken = default)
    {
        var sesion = await _sesionRepository.ObtenerConDetalleAsync(sesionId, cancellationToken);
        if (sesion is null || sesion.IsDeleted || sesion.Estado != EstadoSesionCaja.Abierta)
        {
            return null;
        }

        if (dto.Monto <= 0)
        {
            throw new ReglaNegocioException("El monto del movimiento debe ser mayor que cero.");
        }

        var movimiento = new MovimientoCaja
        {
            SesionCajaId = sesionId,
            Tipo = dto.Tipo,
            Monto = dto.Monto,
            Moneda = dto.Moneda,
            Motivo = dto.Motivo
        };

        await _sesionRepository.AgregarMovimientoAsync(movimiento, cancellationToken);
        await _sesionRepository.SaveChangesAsync(cancellationToken);

        var actualizada = await _sesionRepository.ObtenerConDetalleAsync(sesionId, cancellationToken);
        return actualizada is null ? null : Mapear(actualizada);
    }

    public async Task<SesionCajaDto?> CerrarSesionAsync(
        Guid sesionId,
        CerrarCajaDto dto,
        CancellationToken cancellationToken = default)
    {
        var sesion = await _sesionRepository.ObtenerConDetalleAsync(sesionId, cancellationToken);
        if (sesion is null || sesion.IsDeleted || sesion.Estado != EstadoSesionCaja.Abierta)
        {
            return null;
        }

        var totalIngresos = sesion.Movimientos.Where(m => m.Tipo == TipoMovimientoCaja.Ingreso).Sum(m => m.Monto);
        var totalEgresos = sesion.Movimientos.Where(m => m.Tipo == TipoMovimientoCaja.Egreso).Sum(m => m.Monto);
        var esperado = sesion.FondoInicial + totalIngresos - totalEgresos;

        decimal contado = 0;

        foreach (var linea in dto.Arqueo)
        {
            var denominacion = await _denominacionRepository.GetByIdAsync(linea.DenominacionId, cancellationToken)
                ?? throw new NoEncontradoException($"No existe la denominación {linea.DenominacionId}.");

            if (linea.Cantidad < 0)
            {
                throw new ReglaNegocioException("La cantidad de una denominación no puede ser negativa.");
            }

            var subtotal = denominacion.Valor * linea.Cantidad;
            contado += subtotal;

            await _sesionRepository.AgregarArqueoAsync(new ArqueoDenominacion
            {
                SesionCajaId = sesionId,
                DenominacionId = denominacion.Id,
                Cantidad = linea.Cantidad,
                Subtotal = subtotal
            }, cancellationToken);
        }

        sesion.Cerrar(esperado, contado, _reloj.UtcNow);
        await _sesionRepository.SaveChangesAsync(cancellationToken);

        var cerrada = await _sesionRepository.ObtenerConDetalleAsync(sesionId, cancellationToken);
        return cerrada is null ? null : Mapear(cerrada);
    }

    public async Task<CierreCajaDto?> ObtenerCierreAsync(Guid sesionId, CancellationToken cancellationToken = default)
    {
        var sesion = await _sesionRepository.ObtenerConDetalleAsync(sesionId, cancellationToken);
        if (sesion is null)
        {
            return null;
        }

        var movimientos = sesion.Movimientos.Select(MapearMovimiento).ToList();
        var totalIngresos = sesion.Movimientos.Where(m => m.Tipo == TipoMovimientoCaja.Ingreso).Sum(m => m.Monto);
        var totalEgresos = sesion.Movimientos.Where(m => m.Tipo == TipoMovimientoCaja.Egreso).Sum(m => m.Monto);

        return new CierreCajaDto(
            sesion.Id,
            sesion.AbiertaEn,
            sesion.CerradaEn,
            sesion.FondoInicial,
            totalIngresos,
            totalEgresos,
            sesion.MontoEsperado,
            sesion.MontoContado,
            sesion.Descuadre,
            movimientos,
            sesion.Arqueos.Select(MapearArqueo).ToList());
    }

    private static SesionCajaDto Mapear(SesionCaja s)
        => new(
            s.Id,
            s.Estado,
            s.FondoInicial,
            s.MontoEsperado,
            s.MontoContado,
            s.Descuadre,
            s.AbiertaEn,
            s.CerradaEn,
            s.Movimientos.Select(MapearMovimiento).ToList(),
            s.Arqueos.Select(MapearArqueo).ToList());

    private static MovimientoCajaDto MapearMovimiento(MovimientoCaja m)
        => new(m.Id, m.Tipo, m.Monto, m.Moneda, m.Motivo, m.CreatedAt);

    private static ArqueoLineaDto MapearArqueo(ArqueoDenominacion a)
        => new(a.DenominacionId, a.Denominacion?.Moneda ?? Moneda.USD, a.Denominacion?.Valor ?? 0m, a.Cantidad, a.Subtotal);
}
