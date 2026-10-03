using Licoreria.Application.Common;
using Licoreria.Application.Dtos;
using Licoreria.Application.Interfaces;
using Licoreria.Domain.Common;
using Licoreria.Domain.Entities;
using Licoreria.Domain.Enums;

namespace Licoreria.Application.Services;

public sealed class ServicioFinanzas : IServicioFinanzas
{
    private readonly ITasaCambioRepository _tasaRepository;
    private readonly IMovimientoTesoreriaRepository _tesoreriaRepository;
    private readonly IRelojSistema _reloj;

    public ServicioFinanzas(
        ITasaCambioRepository tasaRepository,
        IMovimientoTesoreriaRepository tesoreriaRepository,
        IRelojSistema reloj)
    {
        _tasaRepository = tasaRepository;
        _tesoreriaRepository = tesoreriaRepository;
        _reloj = reloj;
    }

    public IReadOnlyList<MonedaDto> ObtenerMonedas() =>
    [
        new(Moneda.USD.ToString(), "Dólar estadounidense"),
        new(Moneda.BS.ToString(), "Bolívar")
    ];

    public async Task<TasaCambioDto?> ObtenerTasaActualAsync(TipoTasa tipo, CancellationToken cancellationToken = default)
    {
        var tasa = await _tasaRepository.ObtenerVigenteAsync(tipo, cancellationToken);
        return tasa is null ? null : Mapear(tasa);
    }

    public async Task<decimal> ObtenerValorVigenteAsync(TipoTasa tipo, CancellationToken cancellationToken = default)
    {
        var tasa = await _tasaRepository.ObtenerVigenteAsync(tipo, cancellationToken)
            ?? throw new NoEncontradoException($"No hay una tasa de cambio vigente para el tipo {tipo}.");
        return tasa.Valor;
    }

    public async Task<IReadOnlyList<TasaCambioDto>> ObtenerHistoricoAsync(
        DateTime? desde = null,
        DateTime? hasta = null,
        TipoTasa? tipo = null,
        CancellationToken cancellationToken = default)
    {
        var tasas = await _tasaRepository.ObtenerHistoricoAsync(desde, hasta, tipo, cancellationToken);
        return tasas.Select(Mapear).ToList();
    }

    public async Task<TasaCambioDto> RegistrarTasaAsync(RegistrarTasaDto dto, CancellationToken cancellationToken = default)
    {
        if (dto.Valor <= 0)
        {
            throw new ReglaNegocioException("La tasa de cambio debe ser mayor que cero.");
        }

        var fecha = (dto.Fecha ?? _reloj.UtcNow).Date;
        var existente = await _tasaRepository.ObtenerPorFechaAsync(fecha, dto.Tipo, cancellationToken);

        if (existente is not null)
        {
            existente.ActualizarValor(dto.Valor);
        }
        else
        {
            existente = new TasaCambio { Fecha = fecha, Tipo = dto.Tipo, Valor = dto.Valor };
            await _tasaRepository.AgregarAsync(existente, cancellationToken);
        }

        await _tasaRepository.SaveChangesAsync(cancellationToken);
        return Mapear(existente);
    }

    public async Task<ResultadoPaginado<MovimientoTesoreriaDto>> ObtenerMovimientosAsync(
        PaginacionRequest paginacion,
        TipoMovimientoTesoreria? tipo = null,
        DateTime? desde = null,
        DateTime? hasta = null,
        CancellationToken cancellationToken = default)
    {
        var pagina = await _tesoreriaRepository.ObtenerPaginadoAsync(paginacion, tipo, desde, hasta, cancellationToken);
        var items = pagina.Items.Select(MapearMovimiento).ToList();
        return ResultadoPaginado<MovimientoTesoreriaDto>.Crear(items, pagina.Page, pagina.PageSize, pagina.TotalItems);
    }

    public async Task<MovimientoTesoreriaDto> RegistrarMovimientoAsync(
        RegistrarMovimientoTesoreriaDto dto,
        CancellationToken cancellationToken = default)
    {
        if (dto.Monto <= 0)
        {
            throw new ReglaNegocioException("El monto del movimiento debe ser mayor que cero.");
        }

        var movimiento = new MovimientoTesoreria
        {
            Tipo = dto.Tipo,
            Monto = dto.Monto,
            Moneda = dto.Moneda,
            Motivo = dto.Motivo,
            ReferenciaTipo = dto.ReferenciaTipo,
            ReferenciaId = dto.ReferenciaId
        };

        await _tesoreriaRepository.AgregarAsync(movimiento, cancellationToken);
        await _tesoreriaRepository.SaveChangesAsync(cancellationToken);
        return MapearMovimiento(movimiento);
    }

    private static TasaCambioDto Mapear(TasaCambio t) => new(t.Id, t.Fecha, t.Tipo, t.Valor);

    private static MovimientoTesoreriaDto MapearMovimiento(MovimientoTesoreria m)
        => new(m.Id, m.Tipo, m.Monto, m.Moneda, m.Motivo, m.ReferenciaTipo, m.ReferenciaId, m.CreatedAt);
}
