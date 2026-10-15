# Design

## Context

Estado actual verificado en el código:

- `backend/tests/Licoreria.UnitTests` tiene 17 archivos y unas 98 pruebas con xUnit 2.9.3 y Moq 4.20.72. Casi todo es de dominio; los servicios con pruebas son `ServicioVentas` (5), `ServicioCuentas` (8), `ServicioCatalogoProducto` (9), planos del Club (6) y reservas del Club (4). Cada archivo declara sus `Mock<>` como campos y un método `CrearServicio()`; para ejecutar `EjecutarEnTransaccionAsync` se configura el mock con `.Returns((operacion, ct) => operacion(ct))`. Convención de nombres: `Metodo_Condicion_Resultado`.
- Sin pruebas: `ServicioInventario` (403 líneas), `ServicioCompras` (438), `ServicioCaja` (236), `ServicioUsuarios` (171), `ServicioAuditoria` (62), `ServicioReportes` (142), `ServicioKardex` (117) y `ServicioAutenticacion` (144).
- `Licoreria.IntegrationTests` (`ApiEndpointsTests`, ~27 `[Fact]` con `[Collection("api")]`) usa `LicoreriaApiFactory` (`WebApplicationFactory<Program>` + `IAsyncLifetime`), que en `InitializeAsync` abre `Host=localhost;Port=5432;Username=licoreria;Password=licoreria`, recrea `licoreria_test`, migra y hace login. Sin PostgreSQL la fixture lanza una excepción y fallan todas las pruebas. El proyecto está en `Licoreria.slnx`, de modo que `dotnet test` lo ejecuta.
- Ambos proyectos ya referencian `coverlet.collector` 6.0.4. No hay `runsettings`, ReportGenerator ni verificación de umbral. `.config/dotnet-tools.json` existe en la raíz del repositorio. El Taskfile tiene `backend:test` (`dotnet test Licoreria.slnx`). `.github/workflows/backend.yml` levanta PostgreSQL como servicio y ejecuta `dotnet test` en Release, sin cobertura. Los scripts auxiliares del repositorio son módulos de Node (`scripts/openapi`), y Node ya está instalado en CI.
- `UnitTests` referencia Domain, Application e Infrastructure (para probar `PasswordHasher` y `TokenService`).
- `ServicioKardex.AplicarAsync` y `ServicioInventario.RegistrarAjusteAsync` se apoyan en `StockProducto.AplicarMovimiento`; `RegistrarRecepcionAsync` depende de `Detalle.Recibir` y del kardex; `CrearUsuarioAsync` verifica el correo con `ExisteEmailAsync` y la base de datos tiene índice único por correo.

Motivación y alcance: ver proposal.md. Comportamiento exigido: ver specs/.

## Goals / Non-Goals

**Goals:**
- Que la lógica de negocio de los servicios sin pruebas quede protegida por pruebas rápidas y deterministas.
- Que `dotnet test` sea verde en cualquier equipo y estricto en CI.
- Que las carreras se puedan probar de forma repetible y se pueda medir y exigir la cobertura.

**Non-Goals:**
- Pruebas de interfaz o de extremo a extremo de navegador, y cobertura del frontend (Vitest queda como está).
- Pruebas con contenedores (Testcontainers) o una base de datos en memoria que sustituya a PostgreSQL en la integración.
- Cobertura de Infrastructure y WebAPI como requisito del umbral (se miden pero no se exigen).
- Corregir defectos del código de producción que las pruebas revelen; se reportan y se tratan por separado.

## Decisions

**D1. Un archivo de pruebas por servicio, siguiendo la convención existente.**
Se crean `ServicioInventarioTests`, `ServicioCajaTests`, `ServicioComprasTests`, `ServicioUsuariosTests`, `ServicioAuditoriaTests`, `ServicioReportesTests`, `ServicioKardexTests` y `ServicioAutenticacionTests` con `Mock<>` como campos, `CrearServicio()` y nombres `Metodo_Condicion_Resultado`. Cada operación pública con reglas de negocio lleva al menos un caso exitoso, uno de borde y uno de error. Las pruebas verifican el resultado observable y las interacciones que importan (por ejemplo, que no se guarda nada ante un error), no cada llamada interna.

**D2. Soporte común en `Support/`.**
`MockExtensions.EjecutarOperaciones(this Mock<IUnitOfWork>)` (la configuración repetida de la transacción), `RelojFijo` (un `Mock<IRelojSistema>` con fecha fija) y `ContextoUsuarioFalso` (usuario, correo, IP y rol configurables) evitan copiar la preparación en cada archivo. Se refactorizan `ServicioVentasTests` y `ServicioCuentasTests` para usarlos solo si el cambio es mecánico y seguro.
*Alternativa:* una clase base de pruebas con herencia (acopla las pruebas y oculta la preparación). Se descarta.

**D3. Separar integración con categoría y omisión por disponibilidad.**
Las pruebas con base de datos usan un atributo propio `HechoDeIntegracionAttribute : FactAttribute` con `[Trait("Category", "Integration")]`, que fija `Skip` cuando la base de datos no está disponible. La disponibilidad se comprueba una sola vez por corrida (conexión a la base de mantenimiento con tiempo máximo de 3 segundos) y se guarda en un valor estático. Si `LICORERIA_TESTS_REQUIRE_DB=true`, el atributo no omite: deja que la prueba falle con un mensaje claro. `LicoreriaApiFactory.InitializeAsync` también consulta esa disponibilidad y no intenta crear la base si no está, para que la fixture no lance antes de que se omitan las pruebas.
*Alternativas:* solo un filtro `--filter` (no protege un `dotnet test` sin filtro, que es lo que un revisor ejecuta); Testcontainers (exige Docker, más lento y más superficie de fallo); sacar el proyecto de la solución (se pierde su compilación en `dotnet build` y en CI).

**D4. Configuración de la conexión por variables de entorno.**
`LICORERIA_TEST_DB` (cadena de conexión de servidor, por defecto la actual) y `LICORERIA_TESTS_REQUIRE_DB` (booleano). El nombre de la base de datos de pruebas sigue siendo `licoreria_test`.

**D5. Tareas del Taskfile.**
`backend:test` pasa a ejecutar solo las unitarias con `--filter "Category!=Integration"`; `backend:test:integration` ejecuta solo la categoría de integración con la base de datos exigida (`LICORERIA_TESTS_REQUIRE_DB=true`); `backend:test:all` ejecuta todo; `backend:coverage` mide y genera el reporte.

**D6. Arnés de concurrencia en `IntegrationTests/Support`.**
`EjecucionConcurrente.EjecutarAsync<T>(int n, Func<int, Task<T>> operacion, TimeSpan? timeout)` crea las `n` tareas, las detiene en un `TaskCompletionSource` y las libera todas a la vez; devuelve una lista de `ResultadoConcurrente<T>` con el valor o la excepción y el tiempo de cada una, y falla con `TimeoutException` indicando qué índices no terminaron. `RepetirAsync(int veces, Func<Task>)` ejecuta el escenario varias veces (10 por defecto) con datos nuevos en cada repetición. Las aserciones comprueban invariantes (existencia final, suma de pagos, cantidad de éxitos) y que ningún fallo sea de tipo error interno (500).
Para la capa unitaria, una extensión de Moq (`ConflictoUnaVez`) configura un repositorio para lanzar `DbUpdateConcurrencyException` en el primer guardado y tener éxito en el segundo, verificando el reintento.
*Alternativa:* sondear con sueños y `Task.Delay` (frágil). Se descarta.

**D7. Alcance de los escenarios de concurrencia.**
Este cambio escribe los escenarios de mermas y ajustes simultáneos, recepción simultánea de una orden, pagos simultáneos de cuentas por pagar y por cobrar y creación simultánea de usuarios, usando la API real y PostgreSQL. Los escenarios de reserva de stock, cierre de cuenta, abonos, reservas de mesa, correlativo y turno de caja pertenecen a `concurrencia-stock` y deben usar este arnés. Las pruebas asumen el 409 de `concurrencia-stock`: si no está aplicado, las que observan 409 fallan, lo cual es la señal buscada.

**D8. Cobertura con la herramienta ya instalada.**
`coverage.runsettings` configura `XPlat Code Coverage` con formato Cobertura, excluye ensamblados de prueba y los archivos `**/Migrations/**`, `**/Program.cs`, `**/SeedData.cs`, `**/*Configuration.cs` y los atributos `GeneratedCode` y `ExcludeFromCodeCoverage`. Los DTOs son `record` y se excluyen por convención de carpeta (`**/Dtos/**`). Se agrega `dotnet-reportgenerator-globaltool` al manifiesto `.config/dotnet-tools.json` para generar HTML y `JsonSummary`.
*Alternativas:* `coverlet.msbuild` con `/p:Threshold` (valida el umbral dentro de la compilación pero duplica la herramienta ya instalada y mezcla dos mecanismos de recolección).

**D9. Umbral verificado por un script de Node.**
`scripts/coverage/verificar-umbral.mjs` lee el `Summary.json` de ReportGenerator, suma líneas cubiertas y válidas solo de los ensamblados Domain y Application, compara con `scripts/coverage/umbral.json` (`{ "lineas": 70 }`, versionado) e imprime medido y exigido. Sale con código distinto de cero si queda por debajo. Se usa Node porque ya está en CI y es el patrón de `scripts/openapi`. El valor inicial (70 %) es una hipótesis: la primera tarea mide la línea base y, si la cobertura final supera con holgura el 70 %, se sube el umbral.

**D10. Integración continua.**
`backend.yml` se divide en pasos: compilar; pruebas unitarias con `--collect:"XPlat Code Coverage" --settings coverage.runsettings`; reporte y verificación del umbral; pruebas de integración con `LICORERIA_TESTS_REQUIRE_DB=true` (el servicio PostgreSQL ya está definido); publicación del reporte como artefacto y del resumen en el resumen del trabajo (`$GITHUB_STEP_SUMMARY`).

**D11. Documentación.**
Una guía `docs/.../pruebas.md` (y enlace desde `CONTRIBUTING.md`) explica las convenciones, las tareas, las variables de entorno, el arnés de concurrencia y cómo leer el reporte, e incluye la tabla de cobertura inicial y final.

## Risks / Trade-offs

- [Pruebas de carrera que fallan de vez en cuando] → Invariantes en lugar de orden, repetición, tiempo máximo explícito y una categoría propia (`Concurrency`) para poder aislarlas en CI si hiciera falta; ninguna aserción depende de cuál operación gana.
- [Pruebas con Moq que prueban la implementación y no el comportamiento] → Se verifican resultados y la ausencia de efectos ante errores; solo se verifican interacciones cuando son el comportamiento (guardar una vez, auditar).
- [Omitir integración en silencio oculta una infraestructura rota] → Omisión solo local; en CI la base de datos es obligatoria y el flujo falla si no está.
- [Un umbral del 70 % puede ser demasiado bajo o alto] → Se fija tras medir la línea base y es un único archivo versionado.
- [Las pruebas pueden revelar defectos reales en producción (por ejemplo devoluciones sin validar la cantidad vendida)] → Se documentan como hallazgos en el reporte de la tarea y se tratan en cambios separados; la prueba correspondiente se escribe marcada como conocida solo si el equipo así lo decide, nunca se debilita la aserción.
- [`dotnet test` más lento si se ejecutan las pruebas de integración] → Las tareas del Taskfile separan ambos grupos y las unitarias no dependen de nada.
- [Cambios de `concurrencia-stock` modifican servicios que estas pruebas cubren] → Este cambio se implementa después y sus pruebas se ajustan a los contratos finales (409, reintento, reservas).

## Migration Plan

1. Medir y registrar la cobertura inicial antes de escribir pruebas.
2. Aplicar primero la separación de integración y las tareas, para tener una base verde.
3. Agregar las pruebas por servicio y el arnés, con la cobertura al lado.
4. Fijar el umbral y activar la verificación en CI al final, con el valor medido.
5. Rollback: eliminar el paso de umbral del flujo y las tareas nuevas; las pruebas no afectan al código de producción.
