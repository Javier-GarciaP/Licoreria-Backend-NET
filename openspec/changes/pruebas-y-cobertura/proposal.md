# Proposal

## Why

El criterio "Pruebas unitarias y cobertura (xUnit y Moq)" fue el más débil de la defensa (6/20) y el docente pidió desarrollar la suite que falta para validar la lógica de negocio. Hoy hay unas 98 pruebas, casi todas de dominio; solo cinco servicios de aplicación tienen pruebas con Moq (Ventas, Cuentas, Catálogo de producto y los planos y reservas del Club). `ServicioInventario`, `ServicioCompras`, `ServicioCaja`, `ServicioUsuarios`, `ServicioAuditoria` y `ServicioReportes` no tienen ninguna, ni tampoco `ServicioKardex` ni `ServicioAutenticacion`. Además, `dotnet test` sobre la solución ejecuta también las pruebas de integración, que exigen un PostgreSQL local con credenciales fijas: sin la base de datos el resultado es rojo, y la rúbrica pide "ejecución 100% verde". `coverlet.collector` está instalado, pero no hay configuración, reporte ni umbral, y la concurrencia (el caso de la última Coca-Cola) no tiene una forma reproducible de probarse.

## What Changes

- **Pruebas unitarias con Moq** de `ServicioInventario`, `ServicioCaja`, `ServicioCompras`, `ServicioUsuarios`, `ServicioAuditoria` y `ServicioReportes`, más `ServicioKardex` (base del inventario) y `ServicioAutenticacion` (JWT y sesiones), cada una con casos exitosos, de borde y de error, y aislamiento total de la base de datos.
- **Soporte común de pruebas**: utilidades para simular la transacción, el reloj y el usuario, para no repetir la preparación de Moq en cada archivo.
- **Pruebas de concurrencia**: un arnés reutilizable que lanza operaciones en paralelo con una señal de salida común, y escenarios de integración sobre los flujos de este cambio (ajustes y mermas simultáneas de la misma variante, recepción simultánea de una orden, pagos simultáneos de una cuenta por pagar o por cobrar, creación simultánea de usuarios con el mismo correo), además de pruebas con Moq que simulan un conflicto al guardar. Los escenarios de reserva de stock, cierre de cuenta, reservas de mesa, correlativo y turno de caja se escriben en `concurrencia-stock` y deben reutilizar este arnés.
- **Separación de la integración**: las pruebas de integración se marcan con una categoría y se omiten (no fallan) cuando no hay PostgreSQL disponible; con `LICORERIA_TESTS_REQUIRE_DB=true` (CI) la ausencia de base de datos sí falla. La cadena de conexión pasa a ser configurable por variable de entorno. `dotnet test` queda en verde en cualquier equipo, y el Taskfile ofrece tareas separadas.
- **Cobertura con coverlet**: configuración de recolección con exclusiones (migraciones, `Program`, DTOs y siembra), reporte HTML y resumen con ReportGenerator, umbral mínimo de líneas para Domain y Application que hace fallar la compilación de CI, y publicación del reporte como artefacto.
- **Documentación** de cómo ejecutar cada grupo de pruebas y medir la cobertura.

## Capabilities

### New Capabilities
- `business-logic-unit-tests`: contrato de cobertura de comportamiento de los servicios de aplicación con pruebas unitarias aisladas con Moq.
- `concurrency-test-suite`: arnés y escenarios reproducibles para verificar invariantes bajo operaciones simultáneas.
- `test-run-separation`: ejecución por defecto de las pruebas sin dependencias externas y ejecución explícita de las de integración.
- `code-coverage-reporting`: medición, reporte y umbral mínimo de cobertura de código.

### Modified Capabilities
<!-- No hay specs principales todavía (openspec/specs está vacío). -->

## Impact

- **Backend (pruebas)**: `Licoreria.UnitTests` (archivos nuevos por servicio y carpeta de soporte), `Licoreria.IntegrationTests` (categoría, atributo de omisión, fábrica, escenarios de concurrencia).
- **Backend (código de producción)**: ninguno, salvo defectos que las pruebas revelen y se corrijan por separado.
- **Herramientas**: `coverage.runsettings`, manifiesto de herramientas de .NET (ReportGenerator), script de verificación del umbral en `scripts/`, tareas del `Taskfile.yml`.
- **CI**: `.github/workflows/backend.yml` (pruebas unitarias con cobertura, integración con base de datos obligatoria, artefacto de cobertura).
- **Documentación**: guía de pruebas en `docs/` y `CONTRIBUTING.md`.
- **Coordinación**: depende de `concurrencia-stock` para que los escenarios de concurrencia observen respuestas 409; las pruebas del interceptor de `auditoria-total` y de `reportes-kpis-export` se escriben en esos cambios y deben respetar las convenciones de este.
