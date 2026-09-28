using FluentValidation;
using Licoreria.Application.Dtos;

namespace Licoreria.Application.Validators;

public sealed class ProductoEditarDtoValidator : AbstractValidator<ProductoEditarDto>
{
    public ProductoEditarDtoValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("El identificador es obligatorio.");

        RuleFor(x => x.Nombre)
            .NotEmpty().WithMessage("El nombre es obligatorio.")
            .MaximumLength(100).WithMessage("El nombre no puede superar los 100 caracteres.");

        RuleFor(x => x.Descripcion)
            .MaximumLength(300).WithMessage("La descripción no puede superar los 300 caracteres.");

        RuleFor(x => x.CodigoBarras)
            .MaximumLength(50).WithMessage("El código de barras no puede superar los 50 caracteres.");

        RuleFor(x => x.PrecioCompraUSD)
            .GreaterThan(0).WithMessage("El precio de compra debe ser mayor que cero.");

        RuleFor(x => x.PrecioVentaUSD)
            .GreaterThan(0).WithMessage("El precio de venta debe ser mayor que cero.")
            .GreaterThanOrEqualTo(x => x.PrecioCompraUSD)
            .WithMessage("El precio de venta no puede ser menor que el de compra.");

        RuleFor(x => x.StockMinimo)
            .GreaterThanOrEqualTo(0).WithMessage("El stock mínimo no puede ser negativo.");

        RuleFor(x => x.StockMaximo)
            .GreaterThan(x => x.StockMinimo)
            .WithMessage("El stock máximo debe ser mayor que el stock mínimo.");
    }
}
