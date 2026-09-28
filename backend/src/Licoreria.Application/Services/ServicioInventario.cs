using Licoreria.Application.Dtos;
using Licoreria.Application.Interfaces;
using Licoreria.Domain.Services;

namespace Licoreria.Application.Services;

public sealed class ServicioInventario : IServicioInventario
{
    private readonly EvaluadorMerma _evaluadorMerma;
    private readonly ConversorMoneda _conversorMoneda;

    public ServicioInventario(EvaluadorMerma evaluadorMerma, ConversorMoneda conversorMoneda)
    {
        _evaluadorMerma = evaluadorMerma;
        _conversorMoneda = conversorMoneda;
    }

    public ResultadoMermaDto EvaluarMerma(MermaRequest request)
    {
        if (!Enum.TryParse<MotivoMerma>(request.Motivo, ignoreCase: true, out var motivo))
        {
            throw new InvalidOperationException($"El motivo de merma '{request.Motivo}' no es válido.");
        }

        var resultado = _evaluadorMerma.Evaluar(motivo, request.Cantidad, request.ReponerSinCobro);

        var movimientos = resultado.Movimientos
            .Select(m => new MovimientoInventarioDto(m.Tipo, m.Cantidad, m.Motivo))
            .ToList();

        return new ResultadoMermaDto(movimientos, resultado.TotalUnidadesDescontadas);
    }

    public decimal ConvertirAusdBolivares(decimal montoUsd, decimal tasaCambio)
        => _conversorMoneda.ConvertirAusdBolivares(montoUsd, tasaCambio);
}
