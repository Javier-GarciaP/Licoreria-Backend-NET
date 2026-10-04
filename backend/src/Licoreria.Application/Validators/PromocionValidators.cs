using FluentValidation;
using Licoreria.Application.Dtos;

namespace Licoreria.Application.Validators;

public sealed class PromocionCrearDtoValidator : AbstractValidator<PromocionCrearDto>
{
    public PromocionCrearDtoValidator()
    {
        RuleFor(x => x.Nombre).NotEmpty().WithMessage("El nombre es obligatorio.")
            .MaximumLength(120).WithMessage("El nombre no puede superar los 120 caracteres.");
        RuleFor(x => x.Tipo).IsInEnum().WithMessage("El tipo de promoción no es válido.");
        RuleFor(x => x.Valor).GreaterThan(0).WithMessage("El valor debe ser mayor que cero.");
    }
}

public sealed class PromocionEditarDtoValidator : AbstractValidator<PromocionEditarDto>
{
    public PromocionEditarDtoValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("El identificador es obligatorio.");
        RuleFor(x => x.Nombre).NotEmpty().WithMessage("El nombre es obligatorio.")
            .MaximumLength(120).WithMessage("El nombre no puede superar los 120 caracteres.");
        RuleFor(x => x.Valor).GreaterThan(0).WithMessage("El valor debe ser mayor que cero.");
    }
}

public sealed class DividirCuentaDtoValidator : AbstractValidator<DividirCuentaDto>
{
    public DividirCuentaDtoValidator()
    {
        RuleFor(x => x)
            .Must(x => (x.Partes is >= 2) || (x.Montos is { Count: > 0 }))
            .WithMessage("Indique al menos dos partes o una lista de montos.");

        RuleForEach(x => x.Montos)
            .GreaterThan(0).WithMessage("Cada monto debe ser mayor que cero.");
    }
}
