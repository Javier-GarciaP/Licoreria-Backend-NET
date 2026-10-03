using FluentValidation;
using Licoreria.Application.Dtos;

namespace Licoreria.Application.Validators;

public sealed class AbrirMesaDtoValidator : AbstractValidator<AbrirMesaDto>
{
    public AbrirMesaDtoValidator()
    {
        RuleFor(x => x.NombreMesa).NotEmpty().WithMessage("El nombre o número de mesa es obligatorio.")
            .MaximumLength(60).WithMessage("El nombre de la mesa no puede superar los 60 caracteres.");
    }
}

public sealed class CrearComandaDtoValidator : AbstractValidator<CrearComandaDto>
{
    public CrearComandaDtoValidator()
    {
        RuleFor(x => x.Area).IsInEnum().WithMessage("El área destino no es válida.");
        RuleFor(x => x.Items).NotEmpty().WithMessage("La comanda debe tener al menos un ítem.");

        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.VarianteId).NotEmpty().WithMessage("La variante es obligatoria.");
            item.RuleFor(i => i.Cantidad).GreaterThan(0).WithMessage("La cantidad debe ser mayor que cero.");
        });
    }
}

public sealed class RegistrarAbonoCuentaDtoValidator : AbstractValidator<RegistrarAbonoCuentaDto>
{
    public RegistrarAbonoCuentaDtoValidator()
    {
        RuleFor(x => x.MetodoPagoId).NotEmpty().WithMessage("El método de pago es obligatorio.");
        RuleFor(x => x.Monto).GreaterThan(0).WithMessage("El monto del abono debe ser mayor que cero.");
    }
}

public sealed class CerrarCuentaDtoValidator : AbstractValidator<CerrarCuentaDto>
{
    public CerrarCuentaDtoValidator()
    {
        RuleFor(x => x.Pagos).NotEmpty().WithMessage("Debe registrar al menos un pago para cerrar la cuenta.");
        RuleFor(x => x.DescuentoUSD).GreaterThanOrEqualTo(0).WithMessage("El descuento no puede ser negativo.");

        RuleForEach(x => x.Pagos).ChildRules(pago =>
        {
            pago.RuleFor(p => p.MetodoPagoId).NotEmpty().WithMessage("El método de pago es obligatorio.");
            pago.RuleFor(p => p.Monto).GreaterThan(0).WithMessage("El monto del pago debe ser mayor que cero.");
        });
    }
}

public sealed class ActualizarEstadoItemDtoValidator : AbstractValidator<ActualizarEstadoItemDto>
{
    public ActualizarEstadoItemDtoValidator()
    {
        RuleFor(x => x.Estado).IsInEnum().WithMessage("El estado del ítem no es válido.");
    }
}
