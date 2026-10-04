using FluentValidation;
using Licoreria.Application.Dtos;

namespace Licoreria.Application.Validators;

public sealed class ZonaEditarDtoValidator : AbstractValidator<ZonaEditarDto>
{
    public ZonaEditarDtoValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("El identificador es obligatorio.");
        RuleFor(x => x.Nombre).NotEmpty().WithMessage("El nombre es obligatorio.")
            .MaximumLength(60).WithMessage("El nombre no puede superar los 60 caracteres.");
        RuleFor(x => x.Tipo).IsInEnum().WithMessage("El tipo de zona no es válido.");
    }
}

public sealed class MesaEditarDtoValidator : AbstractValidator<MesaEditarDto>
{
    public MesaEditarDtoValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("El identificador es obligatorio.");
        RuleFor(x => x.ZonaId).NotEmpty().WithMessage("La zona es obligatoria.");
        RuleFor(x => x.Numero).NotEmpty().WithMessage("El número de mesa es obligatorio.")
            .MaximumLength(20).WithMessage("El número no puede superar los 20 caracteres.");
        RuleFor(x => x.Capacidad).GreaterThan(0).WithMessage("La capacidad debe ser mayor que cero.");
    }
}

public sealed class PlanoEditarDtoValidator : AbstractValidator<PlanoEditarDto>
{
    public PlanoEditarDtoValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("El identificador es obligatorio.");
        RuleFor(x => x.Nombre).NotEmpty().WithMessage("El nombre es obligatorio.")
            .MaximumLength(60).WithMessage("El nombre no puede superar los 60 caracteres.");
    }
}

public sealed class EventoEditarDtoValidator : AbstractValidator<EventoEditarDto>
{
    public EventoEditarDtoValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("El identificador es obligatorio.");
        RuleFor(x => x.Titulo).NotEmpty().WithMessage("El título es obligatorio.")
            .MaximumLength(120).WithMessage("El título no puede superar los 120 caracteres.");
        RuleFor(x => x.Descripcion).MaximumLength(1000).WithMessage("La descripción no puede superar los 1000 caracteres.");
    }
}

public sealed class ClienteEditarDtoValidator : AbstractValidator<ClienteEditarDto>
{
    public ClienteEditarDtoValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("El identificador es obligatorio.");
        RuleFor(x => x.Nombre).NotEmpty().WithMessage("El nombre es obligatorio.")
            .MaximumLength(120).WithMessage("El nombre no puede superar los 120 caracteres.");
        RuleFor(x => x.Email).EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.Email))
            .WithMessage("El correo no tiene un formato válido.");
    }
}

public sealed class PaginaEditarDtoValidator : AbstractValidator<PaginaEditarDto>
{
    public PaginaEditarDtoValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("El identificador es obligatorio.");
        RuleFor(x => x.Titulo).NotEmpty().WithMessage("El título es obligatorio.")
            .MaximumLength(120).WithMessage("El título no puede superar los 120 caracteres.");
        RuleFor(x => x.Slug).NotEmpty().WithMessage("El slug es obligatorio.")
            .MaximumLength(120).WithMessage("El slug no puede superar los 120 caracteres.");
    }
}
