using Licoreria.Domain.Enums;

namespace Licoreria.Application.Dtos;

public sealed record DestaparEnvaseDto(
    Guid PorcionVarianteId,
    Guid PresentacionVarianteId);

public sealed record DescartarEnvaseDto(
    string Motivo);

public sealed record EnvaseAbiertoDto(
    Guid Id,
    Guid PorcionVarianteId,
    string PorcionNombre,
    Guid PresentacionVarianteId,
    string PresentacionNombre,
    int PorcionesIniciales,
    int PorcionesRestantes,
    EstadoEnvase Estado,
    DateTime AbiertoEn,
    Guid? AbiertoPorUsuarioId,
    DateTime? CerradoEn,
    string? MotivoDescarte);
