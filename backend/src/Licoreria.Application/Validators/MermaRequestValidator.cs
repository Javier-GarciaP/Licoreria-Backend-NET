using FluentValidation;
using Licoreria.Application.Dtos;
using Licoreria.Domain.Services;

namespace Licoreria.Application.Validators;

public sealed class MermaRequestValidator : AbstractValidator<MermaRequest>
{
    public MermaRequestValidator()
    {
        RuleFor(x => x.Motivo)
            .NotEmpty().WithMessage("El motivo de la merma es obligatorio.")
            .Must(motivo => Enum.TryParse<MotivoMerma>(motivo, ignoreCase: true, out _))
            .WithMessage("El motivo debe ser Danado, Partido o Vencido.");

        RuleFor(x => x.Cantidad)
            .GreaterThan(0).WithMessage("La cantidad debe ser mayor que cero.");
    }
}
