using FluentValidation;
using Licoreria.Application.Dtos;

namespace Licoreria.Application.Validators;

public sealed class ProductoCrearDtoValidator : AbstractValidator<ProductoCrearDto>
{
    public ProductoCrearDtoValidator()
    {
        RuleFor(x => x.Nombre)
            .NotEmpty().WithMessage("El nombre es obligatorio.")
            .MaximumLength(120).WithMessage("El nombre no puede superar los 120 caracteres.");

        RuleFor(x => x.Descripcion)
            .MaximumLength(300).WithMessage("La descripción no puede superar los 300 caracteres.");

        RuleFor(x => x.CategoriaId)
            .NotEmpty().WithMessage("La categoría es obligatoria.");

        RuleFor(x => x.Variantes)
            .NotEmpty().WithMessage("El producto debe tener al menos una variante.");

        RuleForEach(x => x.Variantes).SetValidator(new VarianteCrearDtoValidator());
    }
}

public sealed class ProductoEditarDtoValidator : AbstractValidator<ProductoEditarDto>
{
    public ProductoEditarDtoValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("El identificador es obligatorio.");

        RuleFor(x => x.Nombre)
            .NotEmpty().WithMessage("El nombre es obligatorio.")
            .MaximumLength(120).WithMessage("El nombre no puede superar los 120 caracteres.");

        RuleFor(x => x.Descripcion)
            .MaximumLength(300).WithMessage("La descripción no puede superar los 300 caracteres.");

        RuleFor(x => x.CategoriaId)
            .NotEmpty().WithMessage("La categoría es obligatoria.");

        RuleFor(x => x.Variantes)
            .NotEmpty().WithMessage("El producto debe tener al menos una variante.");

        RuleForEach(x => x.Variantes).SetValidator(new VarianteCrearDtoValidator());
    }
}

public sealed class VarianteCrearDtoValidator : AbstractValidator<VarianteCrearDto>
{
    public VarianteCrearDtoValidator()
    {
        RuleFor(x => x.Nombre)
            .NotEmpty().WithMessage("El nombre de la variante es obligatorio.")
            .MaximumLength(120).WithMessage("El nombre de la variante no puede superar los 120 caracteres.");

        RuleFor(x => x.Sku)
            .NotEmpty().WithMessage("El SKU es obligatorio.")
            .MaximumLength(50).WithMessage("El SKU no puede superar los 50 caracteres.");

        RuleFor(x => x.UnidadMedidaId)
            .NotEmpty().WithMessage("La unidad de medida es obligatoria.");

        RuleFor(x => x.PrecioCompraUSD)
            .GreaterThanOrEqualTo(0).WithMessage("El precio de compra no puede ser negativo.");

        RuleFor(x => x.PrecioVentaUSD)
            .GreaterThanOrEqualTo(0).WithMessage("El precio de venta no puede ser negativo.")
            .GreaterThanOrEqualTo(x => x.PrecioCompraUSD)
            .WithMessage("El precio de venta no puede ser menor que el de compra.");

        RuleForEach(x => x.CodigosBarras)
            .MaximumLength(50).WithMessage("El código de barras no puede superar los 50 caracteres.");
    }
}
