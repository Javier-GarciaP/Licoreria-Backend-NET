using FluentValidation;
using Licoreria.Application.Dtos;

namespace Licoreria.Application.Validators;

public sealed class SaludStockRequestValidator : AbstractValidator<SaludStockRequest>
{
    public SaludStockRequestValidator()
    {
        RuleFor(x => x.Sku)
            .NotEmpty().WithMessage("El SKU es obligatorio.");

        RuleFor(x => x.StockMinimo)
            .GreaterThanOrEqualTo(0).WithMessage("El stock mínimo no puede ser negativo.");

        RuleFor(x => x.StockMaximo)
            .GreaterThanOrEqualTo(x => x.StockMinimo)
            .WithMessage("El stock máximo debe ser mayor o igual al mínimo.");
    }
}
