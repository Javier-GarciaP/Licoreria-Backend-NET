# Proposal

## Why

En la defensa, el docente pidió nuevos KPIs relevantes, ajustar el tamaño de las gráficas del Dashboard y generar y exportar reportes en PDF y Excel; el criterio "Dashboard KPI y analítica" quedó en 14/20. Hoy el backend ya expone ticket promedio, ventas por método de pago, heatmap, salud de stock y mermas vs ventas, pero faltan los indicadores que cuentan la operación de la discoteca (top productos y rotación, ocupación de mesas, descuadre de caja, desempeño del mesonero), el Dashboard solo tiene periodos fijos sin selector, `ServicioReportes` no tiene pruebas unitarias y no existe ninguna exportación de reportes (solo el PDF del menú). El grupo Analítica está oculto en la navegación.

## What Changes

- **KPIs de ventas y producto**: ticket promedio y ventas por método de pago para un periodo elegido, top productos por unidades e ingresos y rotación de inventario con detección de productos sin movimiento.
- **KPIs de operación del local**: ocupación de mesas (ocupación actual, permanencia promedio y rotación por zona), asistencia de reservas (tasa de no-show), descuadre de caja por turno y mermas por motivo.
- **Desempeño del mesonero**: por mesonero y periodo, comandas, ítems, ventas generadas, ticket por cuenta, tasa de cancelación y tiempo promedio de servicio. Para medir el tiempo de servicio, `ComandaDetalle` registra cuándo se sirvió el ítem (`ServidoEn`).
- **Dashboard con periodo**: un selector de periodo (hoy, 7 días, 30 días, mes y personalizado) gobierna las tarjetas y gráficas; las gráficas pasan a alturas uniformes y legibles.
- **Exportación a PDF y Excel**: cada reporte (ventas, inventario valorizado, compras, propinas, top productos, mesoneros, caja, ocupación, mermas vs ventas y la bitácora de auditoría) se descarga en PDF (QuestPDF, ya instalado) o Excel (`.xlsx`, librería nueva) con los mismos filtros de la pantalla.
- **Analítica reactivada**: el grupo Analítica y el ítem Reportes dejan de estar ocultos para el Administrador; los reportes ganan pestañas nuevas y botones de exportación.
- **Pruebas**: unitarias con Moq de `ServicioReportes`, de los mapeos a tabla y de ambos exportadores, e integración de las consultas nuevas con datos sembrados.

## Capabilities

### New Capabilities
- `sales-product-kpis`: ticket promedio, ventas por método de pago, top productos y rotación para un periodo.
- `venue-operations-kpis`: ocupación de mesas, asistencia de reservas, descuadre de caja y mermas por motivo.
- `waiter-performance-kpis`: indicadores de desempeño por mesonero, incluido el tiempo de servicio.
- `analytics-dashboard`: Dashboard y Reportes con selector de periodo, gráficas de tamaño uniforme y navegación de Analítica activa.
- `report-export`: descarga de cada reporte en PDF y Excel con los filtros aplicados.

### Modified Capabilities
<!-- No hay specs principales todavía (openspec/specs está vacío). -->

## Impact

- **Backend (Domain)**: `ComandaDetalle.ServidoEn` y su asignación al pasar a Preparado o Entregado.
- **Backend (Application)**: `IServicioReportes` y `ServicioReportes` (consultas nuevas), DTOs de reporte, modelo neutral de tabla de reporte y puerto del exportador.
- **Backend (Infrastructure)**: `ReportesRepository` (agregaciones en base de datos), exportadores QuestPDF y Excel, paquete de Excel (ClosedXML), migración de `ServidoEn`.
- **Backend (WebAPI)**: `ReportesController` (endpoints nuevos y de exportación), `openapi/licoreria.yaml`.
- **Frontend**: `DashboardPage`, `ReportesPage`, `packages/api-client` (nuevos reportes y descarga de archivos), `packages/types`, `lib/navigation.tsx`.
- **Coordinación**: `lib/navigation.tsx` y el grupo `analitica` se comparten con `auditoria-total`; `ComandaDetalle` también cambia en `concurrencia-stock` (campo `Reservado`) y las migraciones deben ordenarse. La exportación de la bitácora depende de los filtros de `auditoria-total`.
