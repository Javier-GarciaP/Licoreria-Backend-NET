using FluentValidation.Results;
using Licoreria.Application.Dtos;
using Licoreria.Application.Validators;
using Licoreria.Domain.Enums;

namespace Licoreria.UnitTests;

/// <summary>Reglas de entrada de caja, compras e inventario: el servicio solo ve DTOs que ya pasaron por aquí.</summary>
public sealed class ValidadoresCajaComprasInventarioTests
{
    private static readonly Guid Id = Guid.NewGuid();

    private static void Rechaza(ValidationResult resultado, string mensaje)
    {
        Assert.False(resultado.IsValid);
        Assert.Contains(resultado.Errors, e => e.ErrorMessage == mensaje);
    }

    // ======================= Caja =======================

    [Theory]
    [InlineData(0, true)]
    [InlineData(50.5, true)]
    [InlineData(-0.01, false)]
    public void AbrirCaja_ElFondoNoPuedeSerNegativo(double fondo, bool valido)
        => Assert.Equal(valido, new AbrirCajaDtoValidator().Validate(new AbrirCajaDto((decimal)fondo)).IsValid);

    [Fact]
    public void MovimientoCaja_Valido_Pasa()
        => Assert.True(new MovimientoCajaCrearDtoValidator()
            .Validate(new MovimientoCajaCrearDto(TipoMovimientoCaja.Egreso, 10m, Moneda.USD, "Compra de hielo")).IsValid);

    [Theory]
    [InlineData(0)]
    [InlineData(-3)]
    public void MovimientoCaja_ElMontoDebeSerPositivo(double monto)
        => Rechaza(
            new MovimientoCajaCrearDtoValidator().Validate(new MovimientoCajaCrearDto(TipoMovimientoCaja.Ingreso, (decimal)monto, Moneda.USD, "x")),
            "El monto debe ser mayor que cero.");

    [Fact]
    public void MovimientoCaja_ElMotivoEsObligatorioYAcotado()
    {
        var validador = new MovimientoCajaCrearDtoValidator();

        Rechaza(validador.Validate(new MovimientoCajaCrearDto(TipoMovimientoCaja.Ingreso, 1m, Moneda.USD, " ")), "El motivo es obligatorio.");
        Assert.True(validador.Validate(new MovimientoCajaCrearDto(TipoMovimientoCaja.Ingreso, 1m, Moneda.USD, new string('a', 200))).IsValid);
        Rechaza(
            validador.Validate(new MovimientoCajaCrearDto(TipoMovimientoCaja.Ingreso, 1m, Moneda.USD, new string('a', 201))),
            "El motivo no puede superar los 200 caracteres.");
    }

    [Fact]
    public void MovimientoCaja_TipoYMonedaDebenSerDelEnumerado()
    {
        var resultado = new MovimientoCajaCrearDtoValidator()
            .Validate(new MovimientoCajaCrearDto((TipoMovimientoCaja)99, 1m, (Moneda)99, "x"));

        Rechaza(resultado, "El tipo de movimiento no es válido.");
        Assert.Contains(resultado.Errors, e => e.ErrorMessage == "La moneda no es válida.");
    }

    [Fact]
    public void CerrarCaja_ExigeArqueoYCantidadesNoNegativas()
    {
        var validador = new CerrarCajaDtoValidator();

        Assert.True(validador.Validate(new CerrarCajaDto([new ArqueoLineaCrearDto(Id, 0)])).IsValid);
        Rechaza(validador.Validate(new CerrarCajaDto([])), "Debe registrar el arqueo por denominaciones.");
        Rechaza(validador.Validate(new CerrarCajaDto([new ArqueoLineaCrearDto(Id, -1)])), "La cantidad no puede ser negativa.");
        Rechaza(validador.Validate(new CerrarCajaDto([new ArqueoLineaCrearDto(Guid.Empty, 1)])), "La denominación es obligatoria.");
    }

    // ======================= Compras =======================

    [Fact]
    public void Proveedor_Crear_ValidaNombreCorreoYDiasDeCredito()
    {
        var validador = new ProveedorCrearDtoValidator();

        Assert.True(validador.Validate(new ProveedorCrearDto("Distribuidora", Email: "ventas@dist.com", DiasCredito: 30)).IsValid);
        Assert.True(validador.Validate(new ProveedorCrearDto("Distribuidora", Email: "")).IsValid);
        Rechaza(validador.Validate(new ProveedorCrearDto("")), "El nombre es obligatorio.");
        Rechaza(validador.Validate(new ProveedorCrearDto(new string('a', 121))), "El nombre no puede superar los 120 caracteres.");
        Rechaza(validador.Validate(new ProveedorCrearDto("X", Email: "no-es-correo")), "El correo no tiene un formato válido.");
        Rechaza(validador.Validate(new ProveedorCrearDto("X", DiasCredito: -1)), "Los días de crédito no pueden ser negativos.");
    }

    [Fact]
    public void Proveedor_Editar_ExigeIdentificadorYNombre()
    {
        var validador = new ProveedorEditarDtoValidator();

        Assert.True(validador.Validate(new ProveedorEditarDto(Id, "X", null, null, null, null, null, 0, true)).IsValid);
        Rechaza(validador.Validate(new ProveedorEditarDto(Guid.Empty, "X", null, null, null, null, null, 0, true)), "El identificador es obligatorio.");
        Rechaza(validador.Validate(new ProveedorEditarDto(Id, " ", null, null, null, null, null, 0, true)), "El nombre es obligatorio.");
        Rechaza(validador.Validate(new ProveedorEditarDto(Id, new string('a', 121), null, null, null, null, null, 0, true)), "El nombre no puede superar los 120 caracteres.");
    }

    [Fact]
    public void OrdenCompra_Crear_ValidaProveedorYLineas()
    {
        var validador = new OrdenCompraCrearDtoValidator();
        OrdenCompraCrearDto Con(Guid proveedor, params OrdenCompraDetalleCrearDto[] lineas) => new(proveedor, lineas);

        Assert.True(validador.Validate(Con(Id, new OrdenCompraDetalleCrearDto(Id, 2m, 0m))).IsValid);
        Rechaza(validador.Validate(Con(Guid.Empty, new OrdenCompraDetalleCrearDto(Id, 1m, 1m))), "El proveedor es obligatorio.");
        Rechaza(validador.Validate(Con(Id)), "La orden debe tener al menos una línea.");
        Rechaza(validador.Validate(Con(Id, new OrdenCompraDetalleCrearDto(Guid.Empty, 1m, 1m))), "La variante es obligatoria.");
        Rechaza(validador.Validate(Con(Id, new OrdenCompraDetalleCrearDto(Id, 0m, 1m))), "La cantidad debe ser mayor que cero.");
        Rechaza(validador.Validate(Con(Id, new OrdenCompraDetalleCrearDto(Id, 1m, -1m))), "El costo no puede ser negativo.");
    }

    [Fact]
    public void Recepcion_Registrar_ValidaOrdenYLineas()
    {
        var validador = new RegistrarRecepcionDtoValidator();
        RegistrarRecepcionDto Con(Guid orden, params RecepcionDetalleCrearDto[] lineas) => new(orden, lineas);

        Assert.True(validador.Validate(Con(Id, new RecepcionDetalleCrearDto(Id, 1m))).IsValid);
        Rechaza(validador.Validate(Con(Guid.Empty, new RecepcionDetalleCrearDto(Id, 1m))), "La orden es obligatoria.");
        Rechaza(validador.Validate(Con(Id)), "La recepción debe tener al menos una línea.");
        Rechaza(validador.Validate(Con(Id, new RecepcionDetalleCrearDto(Guid.Empty, 1m))), "La línea de la orden es obligatoria.");
        Rechaza(validador.Validate(Con(Id, new RecepcionDetalleCrearDto(Id, 0m))), "La cantidad debe ser mayor que cero.");
    }

    [Fact]
    public void PagoProveedor_ExigeMontoPositivoYMonedaValida()
    {
        var validador = new RegistrarPagoProveedorDtoValidator();

        Assert.True(validador.Validate(new RegistrarPagoProveedorDto(10m)).IsValid);
        Rechaza(validador.Validate(new RegistrarPagoProveedorDto(0m)), "El monto del pago debe ser mayor que cero.");
        Rechaza(validador.Validate(new RegistrarPagoProveedorDto(5m, (Moneda)99)), "La moneda no es válida.");
    }

    // ======================= Inventario =======================

    [Fact]
    public void Merma_ValidaVarianteCantidadYMotivo()
    {
        var validador = new RegistrarMermaDtoValidator();

        Assert.True(validador.Validate(new RegistrarMermaDto(Id, 1m, MotivoMerma.Vencido)).IsValid);
        Rechaza(validador.Validate(new RegistrarMermaDto(Guid.Empty, 1m, MotivoMerma.Danado)), "La variante es obligatoria.");
        Rechaza(validador.Validate(new RegistrarMermaDto(Id, 0m, MotivoMerma.Danado)), "La cantidad debe ser mayor que cero.");
        Rechaza(validador.Validate(new RegistrarMermaDto(Id, 1m, (MotivoMerma)99)), "El motivo de merma no es válido.");
    }

    [Fact]
    public void Ajuste_ValidaVarianteCantidadDistintaDeCeroYMotivo()
    {
        var validador = new AjusteInventarioDtoValidator();

        Assert.True(validador.Validate(new AjusteInventarioDto(Id, -5m, "Conteo")).IsValid);
        Rechaza(validador.Validate(new AjusteInventarioDto(Guid.Empty, 1m, "x")), "La variante es obligatoria.");
        Rechaza(validador.Validate(new AjusteInventarioDto(Id, 0m, "x")), "La cantidad del ajuste debe ser distinta de cero.");
        Rechaza(validador.Validate(new AjusteInventarioDto(Id, 1m, "")), "El motivo del ajuste es obligatorio.");
        Rechaza(validador.Validate(new AjusteInventarioDto(Id, 1m, new string('a', 201))), "El motivo no puede superar los 200 caracteres.");
    }

    [Fact]
    public void Lote_Crear_ValidaVarianteCodigoYCantidad()
    {
        var validador = new LoteCrearDtoValidator();

        Assert.True(validador.Validate(new LoteCrearDto(Id, "L-1", 0m)).IsValid);
        Rechaza(validador.Validate(new LoteCrearDto(Guid.Empty, "L-1", 1m)), "La variante es obligatoria.");
        Rechaza(validador.Validate(new LoteCrearDto(Id, "", 1m)), "El código del lote es obligatorio.");
        Rechaza(validador.Validate(new LoteCrearDto(Id, new string('a', 51), 1m)), "El código no puede superar los 50 caracteres.");
        Rechaza(validador.Validate(new LoteCrearDto(Id, "L-1", -1m)), "La cantidad no puede ser negativa.");
    }

    [Fact]
    public void Lote_Editar_ValidaIdentificadorCodigoYCantidad()
    {
        var validador = new LoteEditarDtoValidator();

        Assert.True(validador.Validate(new LoteEditarDto(Id, "L-1", 3m, null, true)).IsValid);
        Rechaza(validador.Validate(new LoteEditarDto(Guid.Empty, "L-1", 3m, null, true)), "El identificador es obligatorio.");
        Rechaza(validador.Validate(new LoteEditarDto(Id, " ", 3m, null, true)), "El código del lote es obligatorio.");
        Rechaza(validador.Validate(new LoteEditarDto(Id, new string('a', 51), 3m, null, true)), "El código no puede superar los 50 caracteres.");
        Rechaza(validador.Validate(new LoteEditarDto(Id, "L-1", -3m, null, true)), "La cantidad no puede ser negativa.");
    }

    [Fact]
    public void TomaFisica_ExigeLineasConVarianteYCantidadContadaNoNegativa()
    {
        var validador = new RegistrarTomaFisicaDtoValidator();

        Assert.True(validador.Validate(new RegistrarTomaFisicaDto([new TomaFisicaDetalleCrearDto(Id, 0m)])).IsValid);
        Rechaza(validador.Validate(new RegistrarTomaFisicaDto([])), "La toma física debe tener al menos una línea.");
        Rechaza(validador.Validate(new RegistrarTomaFisicaDto([new TomaFisicaDetalleCrearDto(Guid.Empty, 1m)])), "La variante es obligatoria.");
        Rechaza(validador.Validate(new RegistrarTomaFisicaDto([new TomaFisicaDetalleCrearDto(Id, -1m)])), "La cantidad contada no puede ser negativa.");
    }
}
