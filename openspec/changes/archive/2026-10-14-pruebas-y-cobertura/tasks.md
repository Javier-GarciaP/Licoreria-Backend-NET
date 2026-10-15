# Tasks

## 1. Separar las pruebas de integración

- [x] 1.1 Crear `HechoDeIntegracionAttribute` (hereda de `FactAttribute`, categoría `Integration`) con la comprobación de disponibilidad de PostgreSQL ejecutada una sola vez, con tiempo máximo de 3 segundos y la variable `LICORERIA_TESTS_REQUIRE_DB`; verificar con pruebas unitarias del atributo (base disponible, no disponible y omitida, no disponible y obligatoria)
- [x] 1.2 Leer la cadena de conexión de `LICORERIA_TEST_DB` (con la actual como valor por defecto) en `LicoreriaApiFactory` y evitar que `InitializeAsync` lance cuando la base no está disponible; verificar ejecutando `dotnet test` con PostgreSQL detenido (integración omitida con motivo visible, resultado verde)
- [x] 1.3 Reemplazar `[Fact]` por `[HechoDeIntegracionAttribute]` en `ApiEndpointsTests`; verificar con `dotnet test --filter "Category=Integration"` con y sin PostgreSQL
- [x] 1.4 Actualizar el `Taskfile.yml` (`backend:test` solo unitarias, `backend:test:integration`, `backend:test:all`); verificar ejecutando cada tarea y comprobando qué pruebas corren
- [x] 1.5 Documentar las variables de entorno y las tareas en `CONTRIBUTING.md`; verificar que las instrucciones se pueden seguir tal cual desde un equipo sin PostgreSQL

## 2. Cobertura: línea base y herramientas

- [x] 2.1 Medir y anotar la cobertura inicial de líneas y ramas por ensamblado antes de agregar pruebas, ejecutando `dotnet test --collect:"XPlat Code Coverage"` sobre `Licoreria.UnitTests`; verificar que el resultado queda registrado en el documento de pruebas
- [x] 2.2 Crear `coverage.runsettings` (formato Cobertura, exclusión de migraciones, `Program`, siembra, configuraciones de EF, DTOs y código generado) y añadir ReportGenerator a `.config/dotnet-tools.json`; verificar que la tarea genera el archivo y que no incluye archivos excluidos
- [x] 2.3 Crear la tarea `backend:coverage` que mide, genera el reporte HTML y `JsonSummary` y muestra el resumen; verificar abriendo el HTML y comprobando que lista Domain, Application e Infrastructure
- [x] 2.4 Escribir `scripts/coverage/verificar-umbral.mjs` y `scripts/coverage/umbral.json`; verificar con pruebas del script sobre un `Summary.json` de ejemplo (por debajo del umbral falla indicando medido y exigido, por encima pasa)

## 3. Soporte común de pruebas

- [x] 3.1 Crear `Support/MockExtensions.EjecutarOperaciones`, `RelojFijo` y `ContextoUsuarioFalso` en `Licoreria.UnitTests`; verificar con pruebas de cada utilidad (la operación se ejecuta, el reloj es fijo, el contexto devuelve los datos configurados)
- [x] 3.2 Refactorizar `ServicioVentasTests` y `ServicioCuentasTests` para usar el soporte común sin cambiar sus aserciones; verificar que siguen pasando con el mismo número de pruebas

## 4. Pruebas unitarias de servicios

- [x] 4.1 `ServicioKardexTests`: movimiento que deja existencia negativa, existencia nueva, desglose de recetas, productos de Cocina sin inventario, variante sin receta y verificación de stock agrupando insumos; verificar con `dotnet test` y la cobertura de `ServicioKardex` por encima del 85 %
- [x] 4.2 `ServicioInventarioTests` de mermas (motivo, movimiento, cortesía opcional, auditoría) y ajustes (signo, existencia insuficiente); verificar con los casos exitoso, de borde y de error de cada operación
- [x] 4.3 `ServicioInventarioTests` de tomas físicas (ajustes solo por líneas con diferencia) y lotes (crear, editar, eliminar, no encontrados); verificar con `dotnet test` y la cobertura de `ServicioInventario` por encima del 75 %
- [x] 4.4 `ServicioCajaTests` de apertura (turno abierto, fondo negativo, usuario no identificado) y de movimientos (monto no positivo, sesión cerrada, ingreso y egreso); verificar con `dotnet test`
- [x] 4.5 `ServicioCajaTests` de cierre (esperado, contado, descuadre positivo y negativo, denominación inexistente, cantidades negativas, desalojo de cuentas con y sin saldo) y estado del turno; verificar con `dotnet test` y la cobertura de `ServicioCaja` por encima del 80 %
- [x] 4.6 `ServicioComprasTests` de proveedores y de órdenes (crear, aprobar, enviar, cancelar y sus transiciones inválidas); verificar con los casos de estado del spec
- [x] 4.7 `ServicioComprasTests` de recepción (orden cancelada, recibida o en borrador, línea ajena, sobre-recepción, recepción parcial y total, kardex, costo, vencimiento por días de crédito) y de pago de cuentas por pagar (pago superior al saldo, pago total); verificar con `dotnet test` y la cobertura de `ServicioCompras` por encima del 75 %
- [x] 4.8 `ServicioUsuariosTests` (crear con correo duplicado y con hash, editar, eliminar lógico, cambiar contraseña con revocación de sesiones y sin exponerla en la auditoría, revocar sesiones, no encontrado); verificar con `dotnet test`
- [x] 4.9 `ServicioAuditoriaTests` (registro con usuario, correo, IP y serialización; consulta paginada con filtros combinados y sin filtros); verificar con `dotnet test`
- [x] 4.10 `ServicioReportesTests` (rango por defecto, rango invertido, límites del dashboard, diagnóstico de inventario con conteos y sugerencias, mermas vs ventas); verificar con `dotnet test`
- [x] 4.11 `ServicioAutenticacionTests` (credenciales válidas, contraseña incorrecta sin revelar el usuario, usuario inactivo, refresco con rotación, token revocado o vencido, cierre de sesión); verificar con `dotnet test`

## 5. Pruebas de concurrencia

- [x] 5.1 Crear el arnés `EjecucionConcurrente` y `RepetirAsync` en `IntegrationTests/Support` con señal de salida común, recogida de resultados y excepciones y tiempo máximo; verificar con pruebas del arnés (arranque simultáneo, resultados completos, tiempo máximo indicando qué operación colgó)
- [x] 5.2 Crear la extensión de Moq `ConflictoUnaVez` y pruebas unitarias de los servicios de este cambio que simulan un conflicto al guardar (ajuste de inventario, recepción de compra, pago de cuenta por pagar); verificar que el servicio reintenta o responde con conflicto sin efectos duplicados
- [x] 5.3 Escenario de integración de mermas y ajustes simultáneos sobre la misma variante (dos mermas sobre la última unidad, ajustes concurrentes) con las invariantes del spec; verificar con `dotnet test --filter "Category=Concurrency"` contra PostgreSQL, repitiendo 10 veces
- [x] 5.4 Escenario de integración de recepción simultánea de la misma línea de una orden (una sola se confirma y la existencia aumenta una vez); verificar con `dotnet test --filter "Category=Concurrency"` repitiendo 10 veces
- [x] 5.5 Escenarios de integración de pagos simultáneos de una cuenta por pagar y de una cuenta por cobrar (el saldo nunca es negativo) y de creación simultánea de usuarios con el mismo correo (201 y 409); verificar con `dotnet test --filter "Category=Concurrency"` repitiendo 10 veces
- [x] 5.6 Documentar en la guía de pruebas cómo escribir un escenario de carrera con el arnés y qué escenarios corresponden a `concurrencia-stock`; verificar que la guía incluye un ejemplo ejecutable

## 6. Umbral, integración continua y documentación

- [x] 6.1 Medir la cobertura final, fijar el umbral en `scripts/coverage/umbral.json` con el valor alcanzado (mínimo 70 % de líneas en Domain y Application) y registrar la comparación inicial y final; verificar que `task backend:coverage` pasa y que baja el umbral artificialmente para comprobar que falla
- [ ] 6.2 Actualizar `.github/workflows/backend.yml` (pruebas unitarias con cobertura, verificación del umbral, integración con `LICORERIA_TESTS_REQUIRE_DB=true`, artefacto del reporte y resumen en el resumen del trabajo); verificar en una ejecución real del flujo que publica el artefacto y que falla con la base de datos detenida
- [x] 6.3 Redactar la guía de pruebas en `docs/` (convenciones, tareas, variables de entorno, arnés, lectura del reporte y tabla de cobertura inicial y final) y enlazarla desde `CONTRIBUTING.md` y el `README`; verificar que `task docs:build` compila
- [x] 6.4 Ejecutar la batería de cierre (`dotnet build`, `dotnet test` sin PostgreSQL y luego con PostgreSQL, `task backend:coverage`) y confirmar que `dotnet test` queda en verde en ambos casos y que la cobertura supera el umbral

## Workflow follow-up

- Revisar los artefactos antes de implementar con `/opsx:apply`.
- Implementar después de `concurrencia-stock` (los escenarios esperan sus respuestas 409) y, idealmente, de `auditoria-total` y `reportes-kpis-export`, para medir la cobertura final con el código definitivo.
- Los hallazgos de defectos que revelen las pruebas se registran y se tratan en cambios separados.
- Archivar el cambio con `/opsx:archive` cuando las pruebas estén en verde.
