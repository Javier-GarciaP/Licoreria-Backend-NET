using FluentValidation;
using Licoreria.Application.Dtos;

namespace Licoreria.Application.Validators;

public sealed class ClienteCrearDtoValidator : AbstractValidator<ClienteCrearDto>
{
    public ClienteCrearDtoValidator()
    {
        RuleFor(x => x.Nombre).NotEmpty().WithMessage("El nombre es obligatorio.")
            .MaximumLength(120).WithMessage("El nombre no puede superar los 120 caracteres.");
        RuleFor(x => x.Email).EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.Email))
            .WithMessage("El correo no tiene un formato válido.");
    }
}

public sealed class PuntosOperacionDtoValidator : AbstractValidator<PuntosOperacionDto>
{
    public PuntosOperacionDtoValidator()
    {
        RuleFor(x => x.Puntos).GreaterThan(0).WithMessage("Los puntos deben ser mayores que cero.");
        RuleFor(x => x.Motivo).NotEmpty().WithMessage("El motivo es obligatorio.")
            .MaximumLength(200).WithMessage("El motivo no puede superar los 200 caracteres.");
    }
}

public sealed class CuentaPorCobrarCrearDtoValidator : AbstractValidator<CuentaPorCobrarCrearDto>
{
    public CuentaPorCobrarCrearDtoValidator()
    {
        RuleFor(x => x.ClienteId).NotEmpty().WithMessage("El cliente es obligatorio.");
        RuleFor(x => x.MontoUSD).GreaterThan(0).WithMessage("El monto debe ser mayor que cero.");
    }
}

public sealed class PagoCuentaPorCobrarDtoValidator : AbstractValidator<PagoCuentaPorCobrarDto>
{
    public PagoCuentaPorCobrarDtoValidator()
    {
        RuleFor(x => x.Monto).GreaterThan(0).WithMessage("El monto del pago debe ser mayor que cero.");
    }
}
