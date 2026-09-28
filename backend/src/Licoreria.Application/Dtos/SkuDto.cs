namespace Licoreria.Application.Dtos;

public sealed record SkuRequest(
    string Categoria,
    string Producto,
    int Secuencia);

public sealed record ResultadoSkuDto(
    string EntradaOriginal,
    string Categoria,
    string NombreSanitizado,
    string SkuGenerado,
    string FormulaSku,
    bool EsValidoEmpresarial);
