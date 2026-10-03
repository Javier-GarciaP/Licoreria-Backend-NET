using FluentValidation;
using Licoreria.Application.Dtos;

namespace Licoreria.Application.Validators;

public sealed class MarcaCrearDtoValidator : AbstractValidator<MarcaCrearDto>
{
    public MarcaCrearDtoValidator()
    {
        RuleFor(x => x.Nombre).NotEmpty().WithMessage("El nombre es obligatorio.")
            .MaximumLength(100).WithMessage("El nombre no puede superar los 100 caracteres.");
        RuleFor(x => x.Descripcion).MaximumLength(200).WithMessage("La descripción no puede superar los 200 caracteres.");
    }
}

public sealed class MarcaEditarDtoValidator : AbstractValidator<MarcaEditarDto>
{
    public MarcaEditarDtoValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("El identificador es obligatorio.");
        RuleFor(x => x.Nombre).NotEmpty().WithMessage("El nombre es obligatorio.")
            .MaximumLength(100).WithMessage("El nombre no puede superar los 100 caracteres.");
        RuleFor(x => x.Descripcion).MaximumLength(200).WithMessage("La descripción no puede superar los 200 caracteres.");
    }
}

public sealed class UnidadMedidaCrearDtoValidator : AbstractValidator<UnidadMedidaCrearDto>
{
    public UnidadMedidaCrearDtoValidator()
    {
        RuleFor(x => x.Nombre).NotEmpty().WithMessage("El nombre es obligatorio.")
            .MaximumLength(50).WithMessage("El nombre no puede superar los 50 caracteres.");
        RuleFor(x => x.Abreviatura).NotEmpty().WithMessage("La abreviatura es obligatoria.")
            .MaximumLength(10).WithMessage("La abreviatura no puede superar los 10 caracteres.");
    }
}

public sealed class UnidadMedidaEditarDtoValidator : AbstractValidator<UnidadMedidaEditarDto>
{
    public UnidadMedidaEditarDtoValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("El identificador es obligatorio.");
        RuleFor(x => x.Nombre).NotEmpty().WithMessage("El nombre es obligatorio.")
            .MaximumLength(50).WithMessage("El nombre no puede superar los 50 caracteres.");
        RuleFor(x => x.Abreviatura).NotEmpty().WithMessage("La abreviatura es obligatoria.")
            .MaximumLength(10).WithMessage("La abreviatura no puede superar los 10 caracteres.");
    }
}

public sealed class ImpuestoCrearDtoValidator : AbstractValidator<ImpuestoCrearDto>
{
    public ImpuestoCrearDtoValidator()
    {
        RuleFor(x => x.Nombre).NotEmpty().WithMessage("El nombre es obligatorio.")
            .MaximumLength(50).WithMessage("El nombre no puede superar los 50 caracteres.");
        RuleFor(x => x.Porcentaje).InclusiveBetween(0, 100).WithMessage("El porcentaje debe estar entre 0 y 100.");
    }
}

public sealed class ImpuestoEditarDtoValidator : AbstractValidator<ImpuestoEditarDto>
{
    public ImpuestoEditarDtoValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("El identificador es obligatorio.");
        RuleFor(x => x.Nombre).NotEmpty().WithMessage("El nombre es obligatorio.")
            .MaximumLength(50).WithMessage("El nombre no puede superar los 50 caracteres.");
        RuleFor(x => x.Porcentaje).InclusiveBetween(0, 100).WithMessage("El porcentaje debe estar entre 0 y 100.");
    }
}

public sealed class ListaPrecioCrearDtoValidator : AbstractValidator<ListaPrecioCrearDto>
{
    public ListaPrecioCrearDtoValidator()
    {
        RuleFor(x => x.Nombre).NotEmpty().WithMessage("El nombre es obligatorio.")
            .MaximumLength(50).WithMessage("El nombre no puede superar los 50 caracteres.");
    }
}

public sealed class ListaPrecioEditarDtoValidator : AbstractValidator<ListaPrecioEditarDto>
{
    public ListaPrecioEditarDtoValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("El identificador es obligatorio.");
        RuleFor(x => x.Nombre).NotEmpty().WithMessage("El nombre es obligatorio.")
            .MaximumLength(50).WithMessage("El nombre no puede superar los 50 caracteres.");
    }
}

public sealed class RecetaCrearDtoValidator : AbstractValidator<RecetaCrearDto>
{
    public RecetaCrearDtoValidator()
    {
        RuleFor(x => x.VarianteInsumoId).NotEmpty().WithMessage("El insumo es obligatorio.");
        RuleFor(x => x.Cantidad).GreaterThan(0).WithMessage("La cantidad debe ser mayor que cero.");
    }
}
