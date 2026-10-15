using FluentValidation.Results;
using Licoreria.Application.Dtos;
using Licoreria.Application.Validators;
using Licoreria.Domain.Enums;

namespace Licoreria.UnitTests;

/// <summary>Reglas de entrada de usuarios, sesión, ventas y cuentas.</summary>
public sealed class ValidadoresUsuariosVentasCuentasTests
{
    private static readonly Guid Id = Guid.NewGuid();

    private static void Rechaza(ValidationResult resultado, string mensaje)
    {
        Assert.False(resultado.IsValid);
        Assert.Contains(resultado.Errors, e => e.ErrorMessage == mensaje);
    }

    // ======================= Sesión y usuarios =======================

    [Fact]
    public void Login_ExigeUsuarioYContrasenaDeAlMenosSeisCaracteres()
    {
        var validador = new LoginDtoValidator();

        Assert.True(validador.Validate(new LoginDto("ana@test.com", "123456")).IsValid);
        Rechaza(validador.Validate(new LoginDto("", "123456")), "El usuario o correo es obligatorio.");
        Rechaza(validador.Validate(new LoginDto("ana@test.com", "")), "La contraseña es obligatoria.");
        Rechaza(validador.Validate(new LoginDto("ana@test.com", "12345")), "La contraseña debe tener al menos 6 caracteres.");
    }

    [Fact]
    public void Usuario_Crear_ValidaNombreCorreoContrasenaYRol()
    {
        var validador = new UsuarioCrearDtoValidator();
        UsuarioCrearDto Con(string nombre = "Ana", string email = "ana@test.com", string clave = "123456", RolUsuario rol = RolUsuario.Cajero)
            => new(nombre, email, clave, rol);

        Assert.True(validador.Validate(Con()).IsValid);
        Rechaza(validador.Validate(Con(nombre: "")), "El nombre completo es obligatorio.");
        Rechaza(validador.Validate(Con(nombre: new string('a', 101))), "El nombre completo no puede superar los 100 caracteres.");
        Rechaza(validador.Validate(Con(email: "")), "El correo es obligatorio.");
        Rechaza(validador.Validate(Con(email: "sin-arroba")), "El correo no tiene un formato válido.");
        Rechaza(validador.Validate(Con(email: new string('a', 95) + "@t.com")), "El correo no puede superar los 100 caracteres.");
        Rechaza(validador.Validate(Con(clave: "")), "La contraseña es obligatoria.");
        Rechaza(validador.Validate(Con(clave: "abc")), "La contraseña debe tener al menos 6 caracteres.");
        Rechaza(validador.Validate(Con(rol: (RolUsuario)99)), "El rol indicado no es válido.");
    }

    [Fact]
    public void Usuario_Editar_ValidaIdentificadorNombreCorreoYRol()
    {
        var validador = new UsuarioEditarDtoValidator();
        UsuarioEditarDto Con(Guid? id = null, string nombre = "Ana", string email = "ana@test.com", RolUsuario rol = RolUsuario.Cajero)
            => new(id ?? Id, nombre, email, rol, true);

        Assert.True(validador.Validate(Con()).IsValid);
        Rechaza(validador.Validate(Con(id: Guid.Empty)), "El identificador es obligatorio.");
        Rechaza(validador.Validate(Con(nombre: " ")), "El nombre completo es obligatorio.");
        Rechaza(validador.Validate(Con(nombre: new string('a', 101))), "El nombre completo no puede superar los 100 caracteres.");
        Rechaza(validador.Validate(Con(email: "")), "El correo es obligatorio.");
        Rechaza(validador.Validate(Con(email: "sin-arroba")), "El correo no tiene un formato válido.");
        Rechaza(validador.Validate(Con(email: new string('a', 95) + "@t.com")), "El correo no puede superar los 100 caracteres.");
        Rechaza(validador.Validate(Con(rol: (RolUsuario)99)), "El rol indicado no es válido.");
    }

    [Fact]
    public void CambiarPassword_ExigeAlMenosSeisCaracteres()
    {
        var validador = new CambiarPasswordDtoValidator();

        Assert.True(validador.Validate(new CambiarPasswordDto("nueva-clave")).IsValid);
        Rechaza(validador.Validate(new CambiarPasswordDto("")), "La nueva contraseña es obligatoria.");
        Rechaza(validador.Validate(new CambiarPasswordDto("12345")), "La nueva contraseña debe tener al menos 6 caracteres.");
    }

    [Fact]
    public void RefreshToken_EsObligatorio()
    {
        var validador = new RefreshTokenRequestValidator();

        Assert.True(validador.Validate(new RefreshTokenRequest("token")).IsValid);
        Rechaza(validador.Validate(new RefreshTokenRequest("")), "El token de refresco es obligatorio.");
    }

    // ======================= Ventas =======================

    private static VentaItemDto Item(Guid? variante = null, decimal cantidad = 1m, decimal descuento = 0m) => new(variante ?? Id, cantidad, DescuentoUSD: descuento);

    private static VentaPagoDto Pago(Guid? metodo = null, decimal monto = 10m) => new(metodo ?? Id, monto);

    private static RegistrarVentaDto Venta(
        IReadOnlyList<VentaItemDto>? items = null,
        IReadOnlyList<VentaPagoDto>? pagos = null,
        decimal descuento = 0m)
        => new(items ?? [Item()], pagos ?? [Pago()], descuento);

    [Fact]
    public void Venta_Valida_Pasa()
        => Assert.True(new RegistrarVentaDtoValidator().Validate(Venta()).IsValid);

    [Fact]
    public void Venta_ExigeItemsPagosYDescuentosNoNegativos()
    {
        var validador = new RegistrarVentaDtoValidator();

        Rechaza(validador.Validate(Venta(items: [])), "La venta debe tener al menos un ítem.");
        Rechaza(validador.Validate(Venta(pagos: [])), "La venta debe registrar al menos un pago.");
        Rechaza(validador.Validate(Venta(descuento: -1m)), "El descuento no puede ser negativo.");
    }

    [Fact]
    public void Venta_ValidaCadaItemYCadaPago()
    {
        var validador = new RegistrarVentaDtoValidator();

        Rechaza(validador.Validate(Venta(items: [Item(variante: Guid.Empty)])), "La variante es obligatoria.");
        Rechaza(validador.Validate(Venta(items: [Item(cantidad: 0m)])), "La cantidad debe ser mayor que cero.");
        Rechaza(validador.Validate(Venta(items: [Item(descuento: -1m)])), "El descuento no puede ser negativo.");
        Rechaza(validador.Validate(Venta(pagos: [Pago(metodo: Guid.Empty)])), "El método de pago es obligatorio.");
        Rechaza(validador.Validate(Venta(pagos: [Pago(monto: 0m)])), "El monto del pago debe ser mayor que cero.");
    }

    [Fact]
    public void PagoVenta_ExigeMetodoYMontoPositivo()
    {
        var validador = new RegistrarPagoVentaDtoValidator();

        Assert.True(validador.Validate(new RegistrarPagoVentaDto(Id, 5m)).IsValid);
        Rechaza(validador.Validate(new RegistrarPagoVentaDto(Guid.Empty, 5m)), "El método de pago es obligatorio.");
        Rechaza(validador.Validate(new RegistrarPagoVentaDto(Id, 0m)), "El monto del pago debe ser mayor que cero.");
    }

    [Fact]
    public void Devolucion_ExigeMotivoDetallesYCantidadesPositivas()
    {
        var validador = new RegistrarDevolucionDtoValidator();
        RegistrarDevolucionDto Con(string motivo = "Producto dañado", params DevolucionDetalleCrearDto[] detalles)
            => new(motivo, true, detalles);

        Assert.True(validador.Validate(Con(detalles: new DevolucionDetalleCrearDto(Id, 1m))).IsValid);
        Rechaza(validador.Validate(Con(motivo: "", new DevolucionDetalleCrearDto(Id, 1m))), "El motivo es obligatorio.");
        Rechaza(validador.Validate(Con(motivo: new string('a', 201), new DevolucionDetalleCrearDto(Id, 1m))), "El motivo no puede superar los 200 caracteres.");
        Rechaza(validador.Validate(Con()), "La devolución debe tener al menos un ítem.");
        Rechaza(validador.Validate(Con(detalles: new DevolucionDetalleCrearDto(Guid.Empty, 1m))), "La variante es obligatoria.");
        Rechaza(validador.Validate(Con(detalles: new DevolucionDetalleCrearDto(Id, 0m))), "La cantidad debe ser mayor que cero.");
    }

    // ======================= Cuentas =======================

    [Fact]
    public void AbrirMesa_ExigeNombreAcotado()
    {
        var validador = new AbrirMesaDtoValidator();

        Assert.True(validador.Validate(new AbrirMesaDto("Mesa 4")).IsValid);
        Rechaza(validador.Validate(new AbrirMesaDto("")), "El nombre o número de mesa es obligatorio.");
        Rechaza(validador.Validate(new AbrirMesaDto(new string('a', 61))), "El nombre de la mesa no puede superar los 60 caracteres.");
    }

    [Fact]
    public void Comanda_ValidaAreaYLineas()
    {
        var validador = new CrearComandaDtoValidator();

        Assert.True(validador.Validate(new CrearComandaDto(AreaDestino.Barra, [new ComandaItemCrearDto(Id, 2m)])).IsValid);
        Rechaza(validador.Validate(new CrearComandaDto((AreaDestino)99, [new ComandaItemCrearDto(Id, 1m)])), "El área destino no es válida.");
        Rechaza(validador.Validate(new CrearComandaDto(AreaDestino.Barra, [])), "La comanda debe tener al menos un ítem.");
        Rechaza(validador.Validate(new CrearComandaDto(AreaDestino.Barra, [new ComandaItemCrearDto(Guid.Empty, 1m)])), "La variante es obligatoria.");
        Rechaza(validador.Validate(new CrearComandaDto(AreaDestino.Barra, [new ComandaItemCrearDto(Id, 0m)])), "La cantidad debe ser mayor que cero.");
    }

    [Fact]
    public void Abono_ExigeMetodoYMontoPositivo()
    {
        var validador = new RegistrarAbonoCuentaDtoValidator();

        Assert.True(validador.Validate(new RegistrarAbonoCuentaDto(Id, 5m)).IsValid);
        Rechaza(validador.Validate(new RegistrarAbonoCuentaDto(Guid.Empty, 5m)), "El método de pago es obligatorio.");
        Rechaza(validador.Validate(new RegistrarAbonoCuentaDto(Id, 0m)), "El monto del abono debe ser mayor que cero.");
    }

    [Fact]
    public void CerrarCuenta_ExigePagosValidosYDescuentoNoNegativo()
    {
        var validador = new CerrarCuentaDtoValidator();

        Assert.True(validador.Validate(new CerrarCuentaDto([Pago()])).IsValid);
        Rechaza(validador.Validate(new CerrarCuentaDto([])), "Debe registrar al menos un pago para cerrar la cuenta.");
        Rechaza(validador.Validate(new CerrarCuentaDto([Pago()], DescuentoUSD: -1m)), "El descuento no puede ser negativo.");
        Rechaza(validador.Validate(new CerrarCuentaDto([Pago(metodo: Guid.Empty)])), "El método de pago es obligatorio.");
        Rechaza(validador.Validate(new CerrarCuentaDto([Pago(monto: 0m)])), "El monto del pago debe ser mayor que cero.");
    }

    [Fact]
    public void EstadoDelItem_DebeSerDelEnumerado()
    {
        var validador = new ActualizarEstadoItemDtoValidator();

        Assert.True(validador.Validate(new ActualizarEstadoItemDto(EstadoItemComanda.Preparado)).IsValid);
        Rechaza(validador.Validate(new ActualizarEstadoItemDto((EstadoItemComanda)99)), "El estado del ítem no es válido.");
    }
}
