using FluentValidation;
using Licoreria.Application.Dtos;

namespace Licoreria.Application.Validators;

public sealed class PaginaCrearDtoValidator : AbstractValidator<PaginaCrearDto>
{
    public PaginaCrearDtoValidator()
    {
        RuleFor(x => x.Titulo).NotEmpty().WithMessage("El título es obligatorio.")
            .MaximumLength(120).WithMessage("El título no puede superar los 120 caracteres.");
        RuleFor(x => x.Slug).NotEmpty().WithMessage("El slug es obligatorio.")
            .MaximumLength(120).WithMessage("El slug no puede superar los 120 caracteres.");
    }
}

public sealed class HorarioGuardarDtoValidator : AbstractValidator<HorarioGuardarDto>
{
    public HorarioGuardarDtoValidator()
    {
        RuleFor(x => x.DiaSemana).InclusiveBetween(1, 7)
            .WithMessage("El día de la semana debe estar entre 1 y 7.");
    }
}

public sealed class LocalInfoEditarDtoValidator : AbstractValidator<LocalInfoEditarDto>
{
    public LocalInfoEditarDtoValidator()
    {
        RuleFor(x => x.Nombre).NotEmpty().WithMessage("El nombre del local es obligatorio.")
            .MaximumLength(120).WithMessage("El nombre no puede superar los 120 caracteres.");
    }
}
