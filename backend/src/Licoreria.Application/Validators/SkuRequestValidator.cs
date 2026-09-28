using FluentValidation;
using Licoreria.Application.Dtos;

namespace Licoreria.Application.Validators;

public sealed class SkuRequestValidator : AbstractValidator<SkuRequest>
{
    public SkuRequestValidator()
    {
        RuleFor(x => x.Categoria)
            .NotEmpty().WithMessage("La categoría es obligatoria.");

        RuleFor(x => x.Producto)
            .NotEmpty().WithMessage("El nombre del producto es obligatorio.");

        RuleFor(x => x.Secuencia)
            .InclusiveBetween(1, 9999)
            .WithMessage("La secuencia debe estar entre 1 y 9999.");
    }
}
