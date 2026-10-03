namespace Licoreria.Infrastructure.Persistence.Configurations;

/// <summary>
/// Identificadores fijos y fecha base para la siembra determinista de datos.
/// </summary>
internal static class SeedData
{
    public static readonly DateTime Fecha = new(2026, 9, 23, 0, 0, 0, DateTimeKind.Utc);

    // --- Categorías ---
    public static readonly Guid CatLicores = Guid.Parse("11111111-1111-1111-1111-111111111111");
    public static readonly Guid CatGaseosas = Guid.Parse("22222222-2222-2222-2222-222222222222");
    public static readonly Guid CatEnergizantes = Guid.Parse("33333333-3333-3333-3333-333333333333");
    public static readonly Guid CatPasabocas = Guid.Parse("44444444-4444-4444-4444-444444444444");

    // --- Marcas ---
    public static readonly Guid MarcaCacique = Guid.Parse("55555555-5555-5555-5555-555555555551");
    public static readonly Guid MarcaOldParr = Guid.Parse("55555555-5555-5555-5555-555555555552");
    public static readonly Guid MarcaCocaCola = Guid.Parse("55555555-5555-5555-5555-555555555553");
    public static readonly Guid MarcaRedBull = Guid.Parse("55555555-5555-5555-5555-555555555554");
    public static readonly Guid MarcaFritoLay = Guid.Parse("55555555-5555-5555-5555-555555555555");

    // --- Unidades de medida ---
    public static readonly Guid UnidadBotella = Guid.Parse("66666666-6666-6666-6666-666666666661");
    public static readonly Guid UnidadUnidad = Guid.Parse("66666666-6666-6666-6666-666666666662");
    public static readonly Guid UnidadTobo = Guid.Parse("66666666-6666-6666-6666-666666666663");
    public static readonly Guid UnidadPlato = Guid.Parse("66666666-6666-6666-6666-666666666664");

    // --- Impuestos ---
    public static readonly Guid ImpuestoIva = Guid.Parse("77777777-7777-7777-7777-777777777771");
    public static readonly Guid ImpuestoIgtf = Guid.Parse("77777777-7777-7777-7777-777777777772");

    // --- Listas de precio ---
    public static readonly Guid ListaDetal = Guid.Parse("88888888-8888-8888-8888-888888888881");
    public static readonly Guid ListaMayorista = Guid.Parse("88888888-8888-8888-8888-888888888882");

    // --- Usuarios ---
    public static readonly Guid UsuarioAdmin = Guid.Parse("20000000-0000-0000-0000-000000000001");
    public static readonly Guid UsuarioCajero = Guid.Parse("20000000-0000-0000-0000-000000000002");

    // --- Tasas de cambio ---
    public static readonly Guid TasaBcv = Guid.Parse("aaaa0000-0000-0000-0000-000000000001");
    public static readonly Guid TasaParalelo = Guid.Parse("aaaa0000-0000-0000-0000-000000000002");

    // --- Métodos de pago ---
    public static readonly Guid PagoEfectivoUsd = Guid.Parse("bbbb0000-0000-0000-0000-000000000001");
    public static readonly Guid PagoEfectivoBs = Guid.Parse("bbbb0000-0000-0000-0000-000000000002");
    public static readonly Guid PagoPagoMovil = Guid.Parse("bbbb0000-0000-0000-0000-000000000003");
    public static readonly Guid PagoZelle = Guid.Parse("bbbb0000-0000-0000-0000-000000000004");
    public static readonly Guid PagoPunto = Guid.Parse("bbbb0000-0000-0000-0000-000000000005");
    public static readonly Guid PagoCredito = Guid.Parse("bbbb0000-0000-0000-0000-000000000006");
}
