using FluentValidation;
using Licoreria.Application.Dtos;

namespace Licoreria.Application.Validators;

public sealed class ProveedorCrearDtoValidator : AbstractValidator<ProveedorCrearDto>
{
    public ProveedorCrearDtoValidator()
    {
        RuleFor(x => x.Nombre).NotEmpty().WithMessage("El nombre es obligatorio.")
            .MaximumLength(120).WithMessage("El nombre no puede superar los 120 caracteres.");
        RuleFor(x => x.Email).EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.Email))
            .WithMessage("El correo no tiene un formato válido.");
        RuleFor(x => x.DiasCredito).GreaterThanOrEqualTo(0).WithMessage("Los días de crédito no pueden ser negativos.");
    }
}

public sealed class ProveedorEditarDtoValidator : AbstractValidator<ProveedorEditarDto>
{
    public ProveedorEditarDtoValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("El identificador es obligatorio.");
        RuleFor(x => x.Nombre).NotEmpty().WithMessage("El nombre es obligatorio.")
            .MaximumLength(120).WithMessage("El nombre no puede superar los 120 caracteres.");
    }
}

public sealed class OrdenCompraCrearDtoValidator : AbstractValidator<OrdenCompraCrearDto>
{
    public OrdenCompraCrearDtoValidator()
    {
        RuleFor(x => x.ProveedorId).NotEmpty().WithMessage("El proveedor es obligatorio.");
        RuleFor(x => x.Detalles).NotEmpty().WithMessage("La orden debe tener al menos una línea.");

        RuleForEach(x => x.Detalles).ChildRules(linea =>
        {
            linea.RuleFor(l => l.VarianteId).NotEmpty().WithMessage("La variante es obligatoria.");
            linea.RuleFor(l => l.Cantidad).GreaterThan(0).WithMessage("La cantidad debe ser mayor que cero.");
            linea.RuleFor(l => l.CostoUnitarioUSD).GreaterThanOrEqualTo(0).WithMessage("El costo no puede ser negativo.");
        });
    }
}

public sealed class RegistrarRecepcionDtoValidator : AbstractValidator<RegistrarRecepcionDto>
{
    public RegistrarRecepcionDtoValidator()
    {
        RuleFor(x => x.OrdenCompraId).NotEmpty().WithMessage("La orden es obligatoria.");
        RuleFor(x => x.Detalles).NotEmpty().WithMessage("La recepción debe tener al menos una línea.");

        RuleForEach(x => x.Detalles).ChildRules(linea =>
        {
            linea.RuleFor(l => l.OrdenCompraDetalleId).NotEmpty().WithMessage("La línea de la orden es obligatoria.");
            linea.RuleFor(l => l.Cantidad).GreaterThan(0).WithMessage("La cantidad debe ser mayor que cero.");
        });
    }
}

public sealed class RegistrarPagoProveedorDtoValidator : AbstractValidator<RegistrarPagoProveedorDto>
{
    public RegistrarPagoProveedorDtoValidator()
    {
        RuleFor(x => x.Monto).GreaterThan(0).WithMessage("El monto del pago debe ser mayor que cero.");
        RuleFor(x => x.Moneda).IsInEnum().WithMessage("La moneda no es válida.");
    }
}
