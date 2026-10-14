using FluentValidation;
using Licoreria.Application.Dtos;

namespace Licoreria.Application.Validators;

public sealed class AbrirCajaDtoValidator : AbstractValidator<AbrirCajaDto>
{
    public AbrirCajaDtoValidator()
    {
        RuleFor(x => x.FondoInicial).GreaterThanOrEqualTo(0).WithMessage("El fondo inicial no puede ser negativo.");
    }
}

public sealed class MovimientoCajaCrearDtoValidator : AbstractValidator<MovimientoCajaCrearDto>
{
    public MovimientoCajaCrearDtoValidator()
    {
        RuleFor(x => x.Tipo).IsInEnum().WithMessage("El tipo de movimiento no es válido.");
        RuleFor(x => x.Moneda).IsInEnum().WithMessage("La moneda no es válida.");
        RuleFor(x => x.Monto).GreaterThan(0).WithMessage("El monto debe ser mayor que cero.");
        RuleFor(x => x.Motivo).NotEmpty().WithMessage("El motivo es obligatorio.")
            .MaximumLength(200).WithMessage("El motivo no puede superar los 200 caracteres.");
    }
}

public sealed class CerrarCajaDtoValidator : AbstractValidator<CerrarCajaDto>
{
    public CerrarCajaDtoValidator()
    {
        RuleFor(x => x.Arqueo).NotEmpty().WithMessage("Debe registrar el arqueo por denominaciones.");

        RuleForEach(x => x.Arqueo).ChildRules(linea =>
        {
            linea.RuleFor(l => l.DenominacionId).NotEmpty().WithMessage("La denominación es obligatoria.");
            linea.RuleFor(l => l.Cantidad).GreaterThanOrEqualTo(0).WithMessage("La cantidad no puede ser negativa.");
        });
    }
}
