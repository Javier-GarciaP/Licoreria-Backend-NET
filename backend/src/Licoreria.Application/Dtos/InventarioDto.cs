namespace Licoreria.Application.Dtos;

public sealed record MermaRequest(
    string Motivo,
    int Cantidad,
    bool ReponerSinCobro);

public sealed record MovimientoInventarioDto(
    string Tipo,
    int Cantidad,
    string Motivo);

public sealed record ResultadoMermaDto(
    IReadOnlyList<MovimientoInventarioDto> Movimientos,
    int TotalUnidadesDescontadas);

public sealed record AbonoRequest(
    decimal Total,
    decimal TotalAbonado,
    decimal MontoAbono);

public sealed record CuentaRequest(
    IEnumerable<decimal> Consumos,
    IEnumerable<decimal> Abonos);

public sealed record ResumenCuentaDto(
    decimal Total,
    decimal TotalAbonado,
    decimal Saldo,
    bool EstaSaldada);
