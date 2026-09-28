namespace Licoreria.Application.Dtos;

public sealed record TasaRequest(decimal Tasa);

public sealed record TasaActualDto(decimal Tasa);

public sealed record ConversionRequest(decimal MontoUsd);

public sealed record ConversionResultDto(decimal MontoUsd, decimal TasaAplicada, decimal MontoBs);
