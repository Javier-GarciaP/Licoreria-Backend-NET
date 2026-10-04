using FluentValidation;
using Licoreria.Application.Dtos;

namespace Licoreria.Application.Validators;

public sealed class LoteCrearDtoValidator : AbstractValidator<LoteCrearDto>
{
    public LoteCrearDtoValidator()
    {
        RuleFor(x => x.VarianteId).NotEmpty().WithMessage("La variante es obligatoria.");
        RuleFor(x => x.Codigo).NotEmpty().WithMessage("El código del lote es obligatorio.")
            .MaximumLength(50).WithMessage("El código no puede superar los 50 caracteres.");
        RuleFor(x => x.Cantidad).GreaterThanOrEqualTo(0).WithMessage("La cantidad no puede ser negativa.");
    }
}

public sealed class LoteEditarDtoValidator : AbstractValidator<LoteEditarDto>
{
    public LoteEditarDtoValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("El identificador es obligatorio.");
        RuleFor(x => x.Codigo).NotEmpty().WithMessage("El código del lote es obligatorio.")
            .MaximumLength(50).WithMessage("El código no puede superar los 50 caracteres.");
        RuleFor(x => x.Cantidad).GreaterThanOrEqualTo(0).WithMessage("La cantidad no puede ser negativa.");
    }
}

public sealed class RegistrarTomaFisicaDtoValidator : AbstractValidator<RegistrarTomaFisicaDto>
{
    public RegistrarTomaFisicaDtoValidator()
    {
        RuleFor(x => x.Detalles).NotEmpty().WithMessage("La toma física debe tener al menos una línea.");

        RuleForEach(x => x.Detalles).ChildRules(linea =>
        {
            linea.RuleFor(l => l.VarianteId).NotEmpty().WithMessage("La variante es obligatoria.");
            linea.RuleFor(l => l.CantidadContada).GreaterThanOrEqualTo(0).WithMessage("La cantidad contada no puede ser negativa.");
        });
    }
}
