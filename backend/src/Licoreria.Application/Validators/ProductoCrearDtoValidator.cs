using FluentValidation;
using Licoreria.Application.Dtos;

namespace Licoreria.Application.Validators;

public sealed class ProductoCrearDtoValidator : AbstractValidator<ProductoCrearDto>
{
    public ProductoCrearDtoValidator()
    {
        RuleFor(x => x.Nombre)
            .NotEmpty().WithMessage("El nombre es obligatorio.")
            .MaximumLength(100).WithMessage("El nombre no puede superar los 100 caracteres.");

        RuleFor(x => x.Descripcion)
            .MaximumLength(300).WithMessage("La descripción no puede superar los 300 caracteres.");

        RuleFor(x => x.Sku)
            .NotEmpty().WithMessage("El SKU es obligatorio.")
            .MaximumLength(20).WithMessage("El SKU no puede superar los 20 caracteres.");

        RuleFor(x => x.CodigoBarras)
            .MaximumLength(50).WithMessage("El código de barras no puede superar los 50 caracteres.");

        RuleFor(x => x.PrecioCompraUSD)
            .GreaterThan(0).WithMessage("El precio de compra debe ser mayor que cero.");

        RuleFor(x => x.PrecioVentaUSD)
            .GreaterThan(0).WithMessage("El precio de venta debe ser mayor que cero.")
            .GreaterThanOrEqualTo(x => x.PrecioCompraUSD)
            .WithMessage("El precio de venta no puede ser menor que el de compra.");

        RuleFor(x => x.Stock)
            .GreaterThanOrEqualTo(0).WithMessage("El stock no puede ser negativo.");

        RuleFor(x => x.StockMinimo)
            .GreaterThanOrEqualTo(0).WithMessage("El stock mínimo no puede ser negativo.");

        RuleFor(x => x.StockMaximo)
            .GreaterThan(x => x.StockMinimo)
            .WithMessage("El stock máximo debe ser mayor que el stock mínimo.");

        RuleFor(x => x.CategoriaId)
            .NotEmpty().WithMessage("La categoría es obligatoria.");

        RuleFor(x => x.MarcaId)
            .NotEmpty().WithMessage("La marca es obligatoria.");

        RuleFor(x => x.UnidadMedidaId)
            .NotEmpty().WithMessage("La unidad de medida es obligatoria.");
    }
}
