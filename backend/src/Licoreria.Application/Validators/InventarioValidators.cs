using FluentValidation;
using Licoreria.Application.Dtos;

namespace Licoreria.Application.Validators;

public sealed class RegistrarMermaDtoValidator : AbstractValidator<RegistrarMermaDto>
{
    public RegistrarMermaDtoValidator()
    {
        RuleFor(x => x.VarianteId).NotEmpty().WithMessage("La variante es obligatoria.");
        RuleFor(x => x.Cantidad).GreaterThan(0).WithMessage("La cantidad debe ser mayor que cero.");
        RuleFor(x => x.Motivo).IsInEnum().WithMessage("El motivo de merma no es válido.");
    }
}

public sealed class AjusteInventarioDtoValidator : AbstractValidator<AjusteInventarioDto>
{
    public AjusteInventarioDtoValidator()
    {
        RuleFor(x => x.VarianteId).NotEmpty().WithMessage("La variante es obligatoria.");
        RuleFor(x => x.Cantidad).NotEqual(0).WithMessage("La cantidad del ajuste debe ser distinta de cero.");
        RuleFor(x => x.Motivo).NotEmpty().WithMessage("El motivo del ajuste es obligatorio.")
            .MaximumLength(200).WithMessage("El motivo no puede superar los 200 caracteres.");
    }
}
