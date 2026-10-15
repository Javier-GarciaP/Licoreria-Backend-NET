# Guía de Pruebas

Cómo se prueba el backend, cómo ejecutar cada grupo de pruebas, cómo escribir una prueba de carrera y cómo leer la
cobertura.

## Tipos de pruebas

| Proyecto | Qué prueba | Herramientas | ¿Necesita PostgreSQL? |
| :--- | :--- | :--- | :--- |
| `Licoreria.UnitTests` | Dominio, validadores y servicios de aplicación, con todas sus dependencias simuladas | xUnit 2.9 + Moq 4.20 | No |
| `Licoreria.IntegrationTests` | La API real (`WebApplicationFactory`) contra PostgreSQL, incluidas las carreras de concurrencia | xUnit + Npgsql | Sí (se omiten si no hay) |

### Convenciones

- Un archivo por servicio: `ServicioXxxTests`, con los `Mock<>` como campos y un método `CrearServicio()`.
- Nombres `Metodo_Condicion_Resultado`.
- Cada operación pública con reglas de negocio lleva al menos un caso exitoso, uno de borde y uno de error. Las pruebas
  verifican el resultado observable y las interacciones que son comportamiento (guardar, auditar, no guardar ante un
  error), no cada llamada interna.
- Una prueba unitaria **nunca** abre una conexión a la base de datos, a la red ni al sistema de archivos. El tiempo, el
  usuario y la transacción se simulan con las utilidades de `Licoreria.UnitTests/Support`:

| Utilidad | Para qué sirve |
| :--- | :--- |
| `mock.EjecutarOperaciones()` | La transacción simulada (`IUnitOfWork`) ejecuta la operación recibida y propaga sus excepciones |
| `RelojFijo.Crear()` | `IRelojSistema` con una fecha fija (`RelojFijo.FechaPorDefecto`) |
| `ContextoUsuarioFalso.Crear()` / `.Anonimo()` | `IContextoUsuario` con usuario, correo, IP y rol configurables, o sin sesión |
| `mock.ConflictoUnaVez(r => r.SaveChangesAsync(...))` | El primer guardado lanza `DbUpdateConcurrencyException` y los siguientes tienen éxito |

## Ejecutar las pruebas

| Tarea | Qué ejecuta |
| :--- | :--- |
| `task backend:test` | Solo las unitarias (`--filter "Category!=Integration"`). No necesita PostgreSQL |
| `task backend:test:integration` | Solo las de integración (`Category=Integration`) con la base de datos obligatoria |
| `task backend:test:all` | Todas; las de integración se omiten con un motivo visible si no hay PostgreSQL |
| `task backend:coverage` | Cobertura de las unitarias, reporte HTML y verificación del umbral |

`dotnet test backend/Licoreria.slnx` (sin filtro) termina en verde en un equipo sin PostgreSQL: las unitarias pasan y las
de integración se reportan como omitidas.

### Variables de entorno

| Variable | Efecto | Valor por defecto |
| :--- | :--- | :--- |
| `LICORERIA_TEST_DB` | Cadena de conexión del servidor (sin base de datos); se usa la base `licoreria_test` | `Host=localhost;Port=5432;Username=licoreria;Password=licoreria` |
| `LICORERIA_TESTS_REQUIRE_DB` | Con `true`, la ausencia de PostgreSQL **falla** las pruebas de integración en lugar de omitirlas | `false` (en CI vale `true`) |

La disponibilidad de PostgreSQL se comprueba una sola vez por corrida y expira a los 3 segundos.

Las pruebas con base de datos se marcan con `[HechoDeIntegracion]` (en lugar de `[Fact]`), que añade la categoría
`Integration` y las omite si no hay base. Las de carrera añaden además `[Trait("Category", "Concurrency")]`; para correr
solo esas: `dotnet test backend/Licoreria.slnx --filter "Category=Concurrency"`.

## Probar una carrera con el arnés

`EjecucionConcurrente` (en `IntegrationTests/Support`) lanza N operaciones detenidas por una señal común, las libera a la
vez, recoge el resultado o la excepción de cada una y falla con `TimeoutException` (indicando qué operaciones no
terminaron) si alguna se cuelga. `RepetirAsync` repite el escenario 10 veces para que una carrera ganada por azar no dé
un falso verde.

Reglas para escribir un escenario:

1. **Datos nuevos en cada repetición** (producto, proveedor, correo): el escenario no puede depender del anterior.
2. **Invariantes, no orden de llegada**: nunca se asume cuál operación gana. Se verifica la cantidad de éxitos, la
   existencia o el saldo final.
3. **Quien pierde recibe 409 (conflicto) o 422 (regla de negocio), nunca 5xx.**
4. Un tiempo máximo explícito si el escenario puede bloquearse.

Ejemplo ejecutable (existe en `ApiEndpointsTests.ConcurrenciaInventarioCompras.cs`): dos mermas sobre la última unidad.

```csharp
[Trait("Category", "Concurrency")]
[HechoDeIntegracion]
public async Task Mermas_DosSobreLaUltimaUnidad_UnaSeConfirmaYLaExistenciaQuedaEnCero()
{
    await EjecucionConcurrente.RepetirAsync(10, async () =>
    {
        var (varianteId, sku) = await CrearProductoConStockAsync(5.0m, 1);

        var resultados = await EjecucionConcurrente.EjecutarAsync(2, async _ =>
            (await _factory.Client.PostAsJsonAsync("/api/v1/mermas",
                new { varianteId, cantidad = 1, motivo = "Danado" })).StatusCode);

        VerificarSinErroresInternos(resultados);          // ninguna operación lanzó ni terminó en 5xx
        Assert.Equal(1, resultados.Count(r => Exito(r.Valor)));
        VerificarPerdedores(resultados);                  // la otra recibió 409 o 422
        Assert.Equal(0m, (await ObtenerStockAsync(sku)).Cantidad);
    });
}
```

Para la capa unitaria, `ConflictoUnaVez` simula la carrera en el repositorio. Los servicios de inventario, compras y
cuentas por pagar **no reintentan**: dejan pasar `DbUpdateConcurrencyException` (la API responde 409) sin haber
persistido nada. Los servicios que se ejecutan dentro de `EjecutarEnTransaccionAsync` sí se reintentan (hasta 3 veces)
releyendo los datos; ver `ServicioConflictosConcurrenciaTests`.

### Escenarios y su dueño

| Escenario | Cambio | Dónde está |
| :--- | :--- | :--- |
| Mermas y ajustes simultáneos, recepción de una misma línea, pagos de cuentas por pagar y por cobrar, usuarios con el mismo correo | `pruebas-y-cobertura` | `ApiEndpointsTests.ConcurrenciaInventarioCompras.cs` |
| Reserva de stock (la última Coca-Cola), cierre de cuenta, abonos, reservas de mesa, correlativo y turno de caja | `concurrencia-stock` | `ApiEndpointsTests.Concurrencia.cs` (usa el mismo arnés) |

## Cobertura

`task backend:coverage` mide líneas y ramas de las pruebas unitarias con `coverlet.collector` y
`backend/coverage.runsettings` (formato Cobertura; se excluyen migraciones, `Program`, siembra, configuraciones de EF,
DTOs y código generado), genera el reporte con ReportGenerator y verifica el umbral.

- **Reporte HTML**: `artifacts/coverage/report/index.html`, navegable por ensamblado, clase y método. En CI se publica
  como artefacto `coverage-report`, y el resumen aparece en el resumen del trabajo.
- **Resumen de texto**: `artifacts/coverage/report/Summary.txt` (y `Summary.json`).
- **Umbral**: `scripts/coverage/umbral.json` (hoy **75 %** de líneas combinadas de Domain y Application). Lo verifica
  `scripts/coverage/verificar-umbral.mjs`, que muestra el valor medido y el exigido y sale con error si queda por debajo.
  El umbral es un único archivo versionado: subirlo o bajarlo queda en el historial del repositorio. Sus pruebas:
  `node --test scripts/coverage/verificar-umbral.test.mjs`.
- Infrastructure y WebAPI se miden pero no se exigen: su comportamiento se verifica con las pruebas de integración.
- La cobertura de CI (compilación Release) sale ~1 punto por encima de la local (Debug), porque el compilador
  optimizado descarta algunas líneas.

### Cobertura inicial y final

Línea base medida con 470 pruebas unitarias antes de agregar las de este cambio; resultado final con 699.

| Ensamblado | Líneas (inicial) | Líneas (final) | Ramas (inicial) | Ramas (final) |
| :--- | ---: | ---: | ---: | ---: |
| Licoreria.Domain | 74,8 % (448 / 599) | 83,1 % (498 / 599) | 67,0 % (122 / 182) | 76,4 % (139 / 182) |
| Licoreria.Application | 55,5 % (854 / 1538) | 73,9 % (1136 / 1538) | 53,0 % (141 / 266) | 60,5 % (161 / 266) |
| Licoreria.Infrastructure | 35,7 % (417 / 1167) | 35,7 % (417 / 1167) | 49,3 % (108 / 219) | 49,3 % (108 / 219) |
| **Domain + Application (umbral)** | **60,9 % (1302 / 2137)** | **76,5 % (1634 / 2137)** | | |
| **Total** | **52,0 % (1719 / 3304)** | **62,1 % (2051 / 3304)** | **55,6 % (371 / 667)** | **61,1 % (408 / 667)** |

Servicios de aplicación cubiertos por este cambio (líneas, antes → después): `ServicioCompras` 0 % → 100 %,
`ServicioInventario` 48,7 % → 100 %, `ServicioCaja` 85,7 % → 92,8 %, `ServicioAuditoria` 93,3 % → 100 %. Además de los
servicios, se probaron los validadores de entrada de caja, compras, inventario, usuarios, sesión, ventas y cuentas.

## Hallazgos de las pruebas

Las pruebas revelaron comportamientos del código de producción que se tratan en cambios aparte (este cambio no los
corrige y no se debilitó ninguna aserción para ocultarlos):

- **Merma con reposición sin cobro y existencia insuficiente deja un efecto parcial.** `RegistrarMermaAsync` guarda el
  movimiento de la merma antes de validar el de la cortesía. Con existencia 3 y una merma de 2 con reposición sin
  cobro, la API responde 422 pero la existencia queda en 1 (comprobado contra PostgreSQL) y sin registro de merma ni
  auditoría.
- **El esperado del cierre de caja suma montos de distintas monedas.** `CerrarSesionAsync` calcula
  `fondo + ingresos − egresos` sin considerar `Moneda` (observado al leer el código; no cubierto por una prueba).
