using FluentValidation;
using Licoreria.Application.Dtos;

namespace Licoreria.Application.Validators;

public sealed class RegistrarVentaDtoValidator : AbstractValidator<RegistrarVentaDto>
{
    public RegistrarVentaDtoValidator()
    {
        RuleFor(x => x.Items)
            .NotEmpty().WithMessage("La venta debe tener al menos un ítem.");

        RuleFor(x => x.Pagos)
            .NotEmpty().WithMessage("La venta debe registrar al menos un pago.");

        RuleFor(x => x.DescuentoUSD)
            .GreaterThanOrEqualTo(0).WithMessage("El descuento no puede ser negativo.");

        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.VarianteId).NotEmpty().WithMessage("La variante es obligatoria.");
            item.RuleFor(i => i.Cantidad).GreaterThan(0).WithMessage("La cantidad debe ser mayor que cero.");
            item.RuleFor(i => i.DescuentoUSD).GreaterThanOrEqualTo(0).WithMessage("El descuento no puede ser negativo.");
        });

        RuleForEach(x => x.Pagos).ChildRules(pago =>
        {
            pago.RuleFor(p => p.MetodoPagoId).NotEmpty().WithMessage("El método de pago es obligatorio.");
            pago.RuleFor(p => p.Monto).GreaterThan(0).WithMessage("El monto del pago debe ser mayor que cero.");
        });
    }
}

public sealed class RegistrarPagoVentaDtoValidator : AbstractValidator<RegistrarPagoVentaDto>
{
    public RegistrarPagoVentaDtoValidator()
    {
        RuleFor(x => x.MetodoPagoId).NotEmpty().WithMessage("El método de pago es obligatorio.");
        RuleFor(x => x.Monto).GreaterThan(0).WithMessage("El monto del pago debe ser mayor que cero.");
    }
}

public sealed class RegistrarDevolucionDtoValidator : AbstractValidator<RegistrarDevolucionDto>
{
    public RegistrarDevolucionDtoValidator()
    {
        RuleFor(x => x.Motivo).NotEmpty().WithMessage("El motivo es obligatorio.")
            .MaximumLength(200).WithMessage("El motivo no puede superar los 200 caracteres.");

        RuleFor(x => x.Detalles).NotEmpty().WithMessage("La devolución debe tener al menos un ítem.");

        RuleForEach(x => x.Detalles).ChildRules(detalle =>
        {
            detalle.RuleFor(d => d.VarianteId).NotEmpty().WithMessage("La variante es obligatoria.");
            detalle.RuleFor(d => d.Cantidad).GreaterThan(0).WithMessage("La cantidad debe ser mayor que cero.");
        });
    }
}
