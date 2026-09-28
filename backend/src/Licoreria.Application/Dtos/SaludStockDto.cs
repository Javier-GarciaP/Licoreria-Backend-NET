namespace Licoreria.Application.Dtos;

public sealed record SaludStockRequest(
    string Sku,
    int StockActual,
    int StockMinimo,
    int StockMaximo);

public sealed record InformeSaludStockDto(
    string Sku,
    int StockActual,
    int StockMinimo,
    int StockMaximo,
    string Estado,
    string MensajeDiagnostico,
    int UnidadesCompraSugeridas);
