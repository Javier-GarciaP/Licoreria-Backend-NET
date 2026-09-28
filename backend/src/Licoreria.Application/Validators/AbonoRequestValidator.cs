using FluentValidation;
using Licoreria.Application.Dtos;

namespace Licoreria.Application.Validators;

public sealed class AbonoRequestValidator : AbstractValidator<AbonoRequest>
{
    public AbonoRequestValidator()
    {
        RuleFor(x => x.Total)
            .GreaterThanOrEqualTo(0).WithMessage("El total no puede ser negativo.");

        RuleFor(x => x.TotalAbonado)
            .GreaterThanOrEqualTo(0).WithMessage("El total abonado no puede ser negativo.");

        RuleFor(x => x.MontoAbono)
            .GreaterThan(0).WithMessage("El monto del abono debe ser mayor que cero.");

        RuleFor(x => x)
            .Must(x => x.TotalAbonado + x.MontoAbono <= x.Total)
            .WithMessage("El abono excede el saldo pendiente de la cuenta.");
    }
}
