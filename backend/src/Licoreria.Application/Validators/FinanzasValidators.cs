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
