using FluentValidation;
using Licoreria.Application.Dtos;

namespace Licoreria.Application.Validators;

public sealed class SolicitarSeccionIaDtoValidator : AbstractValidator<SolicitarSeccionIaDto>
{
    public SolicitarSeccionIaDtoValidator()
    {
        RuleFor(x => x.Prompt).NotEmpty().WithMessage("La instrucción es obligatoria.")
            .MaximumLength(1000).WithMessage("La instrucción no puede superar los 1000 caracteres.");
    }
}

public sealed class SolicitarImagenIaDtoValidator : AbstractValidator<SolicitarImagenIaDto>
{
    public SolicitarImagenIaDtoValidator()
    {
        RuleFor(x => x.Prompt).NotEmpty().WithMessage("La instrucción es obligatoria.")
            .MaximumLength(1000).WithMessage("La instrucción no puede superar los 1000 caracteres.");
    }
}
