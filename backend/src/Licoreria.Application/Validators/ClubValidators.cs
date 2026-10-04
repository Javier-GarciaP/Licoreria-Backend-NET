using FluentValidation;
using Licoreria.Application.Dtos;

namespace Licoreria.Application.Validators;

public sealed class ZonaCrearDtoValidator : AbstractValidator<ZonaCrearDto>
{
    public ZonaCrearDtoValidator()
    {
        RuleFor(x => x.Nombre).NotEmpty().WithMessage("El nombre es obligatorio.")
            .MaximumLength(60).WithMessage("El nombre no puede superar los 60 caracteres.");
        RuleFor(x => x.Tipo).IsInEnum().WithMessage("El tipo de zona no es válido.");
    }
}

public sealed class MesaCrearDtoValidator : AbstractValidator<MesaCrearDto>
{
    public MesaCrearDtoValidator()
    {
        RuleFor(x => x.ZonaId).NotEmpty().WithMessage("La zona es obligatoria.");
        RuleFor(x => x.Numero).NotEmpty().WithMessage("El número de mesa es obligatorio.")
            .MaximumLength(20).WithMessage("El número no puede superar los 20 caracteres.");
        RuleFor(x => x.Capacidad).GreaterThan(0).WithMessage("La capacidad debe ser mayor que cero.");
    }
}

public sealed class ReservaCrearDtoValidator : AbstractValidator<ReservaCrearDto>
{
    public ReservaCrearDtoValidator()
    {
        RuleFor(x => x.Personas).GreaterThan(0).WithMessage("El número de personas debe ser mayor que cero.");
        RuleFor(x => x.Mesas).NotEmpty().WithMessage("La reserva debe incluir al menos una mesa.");
        RuleFor(x => x.NombreContacto).NotEmpty().WithMessage("El nombre de contacto es obligatorio.")
            .MaximumLength(120).WithMessage("El nombre no puede superar los 120 caracteres.");
        RuleFor(x => x.Telefono).NotEmpty().WithMessage("El teléfono es obligatorio.")
            .MaximumLength(30).WithMessage("El teléfono no puede superar los 30 caracteres.");
    }
}

public sealed class ReservaPagoCrearDtoValidator : AbstractValidator<ReservaPagoCrearDto>
{
    public ReservaPagoCrearDtoValidator()
    {
        RuleFor(x => x.MetodoPagoId).NotEmpty().WithMessage("El método de pago es obligatorio.");
        RuleFor(x => x.Monto).GreaterThan(0).WithMessage("El monto de la seña debe ser mayor que cero.");
    }
}

public sealed class EventoCrearDtoValidator : AbstractValidator<EventoCrearDto>
{
    public EventoCrearDtoValidator()
    {
        RuleFor(x => x.Titulo).NotEmpty().WithMessage("El título es obligatorio.")
            .MaximumLength(120).WithMessage("El título no puede superar los 120 caracteres.");
        RuleFor(x => x.Descripcion).MaximumLength(1000).WithMessage("La descripción no puede superar los 1000 caracteres.");
    }
}

public sealed class ListaVipCrearDtoValidator : AbstractValidator<ListaVipCrearDto>
{
    public ListaVipCrearDtoValidator()
    {
        RuleFor(x => x.Nombre).NotEmpty().WithMessage("El nombre es obligatorio.")
            .MaximumLength(120).WithMessage("El nombre no puede superar los 120 caracteres.");
    }
}

public sealed class EmitirEntradaDtoValidator : AbstractValidator<EmitirEntradaDto>
{
    public EmitirEntradaDtoValidator()
    {
        RuleFor(x => x.Precio).GreaterThanOrEqualTo(0).WithMessage("El precio no puede ser negativo.");
        RuleFor(x => x.Moneda).IsInEnum().WithMessage("La moneda no es válida.");
    }
}

public sealed class PedidoAnticipadoCrearDtoValidator : AbstractValidator<PedidoAnticipadoCrearDto>
{
    public PedidoAnticipadoCrearDtoValidator()
    {
        RuleFor(x => x.VarianteId).NotEmpty().WithMessage("La variante es obligatoria.");
        RuleFor(x => x.Cantidad).GreaterThan(0).WithMessage("La cantidad debe ser mayor que cero.");
    }
}
