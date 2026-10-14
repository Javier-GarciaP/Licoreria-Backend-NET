using FluentValidation;
using Licoreria.Application.Dtos;

namespace Licoreria.Application.Validators;

public sealed class UsuarioCrearDtoValidator : AbstractValidator<UsuarioCrearDto>
{
    public UsuarioCrearDtoValidator()
    {
        RuleFor(x => x.NombreCompleto)
            .NotEmpty().WithMessage("El nombre completo es obligatorio.")
            .MaximumLength(100).WithMessage("El nombre completo no puede superar los 100 caracteres.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("El correo es obligatorio.")
            .EmailAddress().WithMessage("El correo no tiene un formato válido.")
            .MaximumLength(100).WithMessage("El correo no puede superar los 100 caracteres.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("La contraseña es obligatoria.")
            .MinimumLength(6).WithMessage("La contraseña debe tener al menos 6 caracteres.");

        RuleFor(x => x.Rol)
            .IsInEnum().WithMessage("El rol indicado no es válido.");
    }
}

public sealed class UsuarioEditarDtoValidator : AbstractValidator<UsuarioEditarDto>
{
    public UsuarioEditarDtoValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("El identificador es obligatorio.");

        RuleFor(x => x.NombreCompleto)
            .NotEmpty().WithMessage("El nombre completo es obligatorio.")
            .MaximumLength(100).WithMessage("El nombre completo no puede superar los 100 caracteres.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("El correo es obligatorio.")
            .EmailAddress().WithMessage("El correo no tiene un formato válido.")
            .MaximumLength(100).WithMessage("El correo no puede superar los 100 caracteres.");

        RuleFor(x => x.Rol)
            .IsInEnum().WithMessage("El rol indicado no es válido.");
    }
}

public sealed class CambiarPasswordDtoValidator : AbstractValidator<CambiarPasswordDto>
{
    public CambiarPasswordDtoValidator()
    {
        RuleFor(x => x.PasswordNueva)
            .NotEmpty().WithMessage("La nueva contraseña es obligatoria.")
            .MinimumLength(6).WithMessage("La nueva contraseña debe tener al menos 6 caracteres.");
    }
}

public sealed class RefreshTokenRequestValidator : AbstractValidator<RefreshTokenRequest>
{
    public RefreshTokenRequestValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty().WithMessage("El token de refresco es obligatorio.");
    }
}
