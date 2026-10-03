using FluentValidation;
using Licoreria.Application.Dtos;

namespace Licoreria.Application.Validators;

public sealed class RegistrarTasaDtoValidator : AbstractValidator<RegistrarTasaDto>
{
    public RegistrarTasaDtoValidator()
    {
        RuleFor(x => x.Tipo).IsInEnum().WithMessage("El tipo de tasa no es válido.");
        RuleFor(x => x.Valor).GreaterThan(0).WithMessage("La tasa debe ser mayor que cero.");
    }
}

public sealed class RegistrarMovimientoTesoreriaDtoValidator : AbstractValidator<RegistrarMovimientoTesoreriaDto>
{
    public RegistrarMovimientoTesoreriaDtoValidator()
    {
        RuleFor(x => x.Tipo).IsInEnum().WithMessage("El tipo de movimiento no es válido.");
        RuleFor(x => x.Moneda).IsInEnum().WithMessage("La moneda no es válida.");
        RuleFor(x => x.Monto).GreaterThan(0).WithMessage("El monto debe ser mayor que cero.");
        RuleFor(x => x.Motivo).NotEmpty().WithMessage("El motivo es obligatorio.")
            .MaximumLength(200).WithMessage("El motivo no puede superar los 200 caracteres.");
    }
}
