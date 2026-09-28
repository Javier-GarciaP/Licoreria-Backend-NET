using FluentValidation;
using Licoreria.Application.Dtos;

namespace Licoreria.Application.Validators;

public sealed class CategoriaCrearDtoValidator : AbstractValidator<CategoriaCrearDto>
{
    public CategoriaCrearDtoValidator()
    {
        RuleFor(x => x.Nombre)
            .NotEmpty().WithMessage("El nombre es obligatorio.")
            .MaximumLength(50).WithMessage("El nombre no puede superar los 50 caracteres.");

        RuleFor(x => x.Descripcion)
            .MaximumLength(200).WithMessage("La descripción no puede superar los 200 caracteres.");
    }
}

public sealed class CategoriaEditarDtoValidator : AbstractValidator<CategoriaEditarDto>
{
    public CategoriaEditarDtoValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("El identificador es obligatorio.");

        RuleFor(x => x.Nombre)
            .NotEmpty().WithMessage("El nombre es obligatorio.")
            .MaximumLength(50).WithMessage("El nombre no puede superar los 50 caracteres.");

        RuleFor(x => x.Descripcion)
            .MaximumLength(200).WithMessage("La descripción no puede superar los 200 caracteres.");
    }
}
