# Tasks

## 1. Base de los reportes y pruebas de servicio

- [x] 1.1 Centralizar la validación de periodo (`desde <= hasta`, máximo 366 días, 30 días por defecto) en `ServicioReportes.NormalizarRango` y responder 400 al violarla; verificar con pruebas unitarias de rango invertido, rango excesivo y valores por defecto
- [x] 1.2 Crear las pruebas unitarias con Moq de `ServicioReportes` para los métodos que ya existen (ventas, propinas, compras, heatmap, diagnóstico de inventario, mermas vs ventas); verificar con `dotnet test` que todas pasan sin base de datos

## 2. KPIs de ventas y producto

- [x] 2.1 Añadir en `ReportesRepository` y `IReportesRepository` la consulta de top productos (por variante, no anuladas, con cortesías sin ingreso) y los DTOs `ReporteProductosDto` y `ProductoVendidoDto`; verificar con una prueba de integración sobre ventas sembradas
- [x] 2.2 Implementar en `ServicioReportes` el top N, el orden por unidades o ingresos, el porcentaje del total y la rotación (no calculable con existencia 0, sin movimiento con existencia y sin ventas); verificar con pruebas Moq de cada caso del spec
- [x] 2.3 Exponer `GET /api/v1/reportes/productos` con `desde`, `hasta`, `orden` y `limite` y la política de ventas; verificar con una prueba de integración (200, 400 por rango inválido, 403 sin permiso)
- [x] 2.4 Verificar que el desglose de ventas por método de pago y el ticket promedio coinciden con el spec para el periodo elegido; verificar con una prueba Moq de pagos mixtos USD y Bs

## 3. KPIs de operación del local

- [x] 3.1 Añadir las consultas de ocupación actual por zona, permanencia y sesiones por mesa, y asistencia de reservas, con sus DTOs; verificar con pruebas de integración (sesión abierta cuenta hasta ahora, canceladas aparte)
- [x] 3.2 Implementar en `ServicioReportes` los porcentajes y tasas (ocupación, no asistencia sobre `Asistio + NoAsistio`); verificar con pruebas Moq, incluida la división entre cero
- [x] 3.3 Añadir el reporte de descuadre por turno (cerrados del periodo, total, promedio y mayor en valor absoluto) y la agrupación de mermas por motivo con costo estimado; verificar con pruebas Moq e integración (un turno abierto no cuenta)
- [x] 3.4 Exponer `GET /api/v1/reportes/ocupacion`, `/reservas-asistencia` y `/caja` con sus políticas y ampliar `mermas-vs-ventas`; verificar con pruebas de integración de estado y permisos

## 4. Desempeño del mesonero

- [x] 4.1 Añadir `ServidoEn` a `ComandaDetalle` y asignarlo la primera vez que pasa a Preparado o Entregado sin sobrescribirlo; verificar con pruebas unitarias de dominio (primer servicio, Preparado → Entregado, ítem cancelado)
- [x] 4.2 Crear la migración de `ServidoEn` (nullable, sin relleno) y verificar que se aplica sobre una BD con comandas previas sin errores
- [x] 4.3 Añadir la consulta por mesonero con `Comanda.CreatedBy` (comandas, ítems, ventas generadas, cuentas, tasa de cancelación y tiempo de servicio, con "Sin asignar" para creador nulo) y su DTO; verificar con una prueba de integración con dos mesoneros y un cajero que cobra
- [x] 4.4 Implementar en `ServicioReportes` el ticket por cuenta, la tasa de cancelación y el promedio de servicio ignorando ítems sin `ServidoEn`; verificar con pruebas Moq de los escenarios del spec
- [x] 4.5 Exponer `GET /api/v1/reportes/mesoneros` con `Roles = "Admin"`; verificar con pruebas de integración (200 para administrador, 403 para cajero)

## 5. Exportación a PDF y Excel

- [x] 5.1 Definir `TablaReporte`, `ColumnaReporte` y el puerto `IExportadorReportes`, y los mapeadores a tabla de cada reporte exportable; verificar con pruebas unitarias de que cada mapeador produce las columnas, filas y totales esperados
- [x] 5.2 Implementar `ExportadorReportesPdf` con QuestPDF (A4, horizontal con más de 6 columnas, encabezado repetido, pie con numeración, metadatos del reporte); verificar con una prueba que el resultado empieza con `%PDF` y que un reporte de más de una página mantiene el encabezado
- [x] 5.3 Añadir ClosedXML a `Infrastructure` (versión estable vigente) e implementar `ExportadorReportesExcel` con celdas numéricas y de fecha con formato, encabezado congelado y filtro automático; verificar abriendo el archivo con ClosedXML en una prueba y comprobando que los importes son numéricos
- [x] 5.4 Implementar `ServicioExportacionReportes` con los límites (5 000 filas PDF → 422, 100 000 Excel), el nombre `reporte-<id>-<desde>_<hasta>` y el tipo de contenido; verificar con pruebas Moq de formato inválido, límite superado y nombre del archivo
- [x] 5.5 Registrar los servicios en `DependencyInjection` y exponer `GET /api/v1/reportes/{reporte}/exportar` heredando la política de cada consulta; verificar con pruebas de integración (descarga de xlsx y pdf, 403 sin permiso, 400 con formato desconocido)
- [x] 5.6 Añadir la exportación de la bitácora de auditoría (Administrador) reutilizando los filtros de `auditoria-total`; verificar con una prueba de integración una vez aplicado ese cambio
- [x] 5.7 Actualizar `openapi/licoreria.yaml` con los endpoints nuevos; verificar con `task backend:openapi:check`

## 6. Frontend

- [x] 6.1 Añadir los tipos y los métodos de `reportesApi` para los reportes nuevos y `apiDownload` en `packages/api-client` (token, `Blob`, nombre desde `Content-Disposition`); verificar con pruebas Vitest con msw de la descarga y del manejo de errores 403 y 422
- [x] 6.2 Crear `SelectorPeriodo` con periodos predefinidos y rango personalizado validado, con el estado en la URL; verificar con pruebas Vitest de cada periodo y del rango invertido
- [x] 6.3 Rediseñar `DashboardPage` para usar el periodo común, las tarjetas de KPIs nuevos con estado vacío y gráficas de altura uniforme con esqueleto; verificar con pruebas Vitest de carga, vacío y cambio de periodo, y visualmente en 375 px y en escritorio
- [x] 6.4 Ampliar `ReportesPage` con las pestañas productos, mesoneros (solo administrador), caja, ocupación y mermas, el selector común y el botón de exportar PDF y Excel; verificar con pruebas Vitest de pestañas, visibilidad por rol y disparo de la descarga
- [x] 6.5 Quitar `oculto` del grupo `analitica` y del ítem `/reportes` en `lib/navigation.tsx`, sin volver a ocultar `/auditoria`; verificar con la prueba de navegación ampliada (el administrador ve Analítica, el cajero no)

## 7. Documentación y cierre

- [x] 7.1 Documentar las definiciones de cada KPI y la exportación en `docs/` y en el `CHANGELOG.md`; verificar que `task docs:build` compila
- [x] 7.2 Ejecutar la batería de cierre (`dotnet build`, `dotnet test`, `npm run typecheck`, `npm run lint`, `npm test`, `npm run build`) y confirmar que todo queda en verde
- [x] 7.3 Recorrer en el stack levantado el Dashboard con cada periodo y exportar un reporte en PDF y otro en Excel, comprobando los totales contra la pantalla

## Workflow follow-up

- Revisar los artefactos antes de implementar con `/opsx:apply`.
- Implementar después de `concurrencia-stock` y `auditoria-total`; la tarea 5.6 depende de este último.
- Coordinar la edición de `lib/navigation.tsx` y el orden de migraciones con los otros cambios.
- Archivar el cambio con `/opsx:archive` cuando las pruebas estén en verde.
