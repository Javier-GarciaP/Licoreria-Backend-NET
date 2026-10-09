using Licoreria.Application.Dtos;
using Licoreria.Application.Interfaces;
using Licoreria.Domain.Common;
using Licoreria.Domain.Entities;
using Licoreria.Domain.Enums;

namespace Licoreria.Application.Services;

public sealed class ServicioFinanzas : IServicioFinanzas
{
    private readonly ITasaCambioRepository _tasaRepository;
    private readonly IRelojSistema _reloj;

    public ServicioFinanzas(
        ITasaCambioRepository tasaRepository,
        IRelojSistema reloj)
    {
        _tasaRepository = tasaRepository;
        _reloj = reloj;
    }

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

    private static TasaCambioDto Mapear(TasaCambio t) => new(t.Id, t.Fecha, t.Tipo, t.Valor);
}
