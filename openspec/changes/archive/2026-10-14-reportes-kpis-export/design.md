# Design

## Context

Estado actual verificado en el código:

- `IServicioReportes` expone dashboard, propinas, ventas (con `PorDia`, `PorUsuario`, `PorMetodoPago` y `TicketPromedioUSD`), inventario valorizado, compras, heatmap, diagnóstico de inventario y mermas vs ventas. `ServicioReportes` es una capa fina (142 líneas) sobre `IReportesRepository`; las agregaciones viven en `ReportesRepository` (357 líneas) con consultas EF sobre el contexto. No hay pruebas unitarias de `ServicioReportes`.
- `ReportesController` usa políticas por permiso (`VentasLeer`, `InventarioLeer`, `ComprasLeer`) y `Roles = "Admin"` solo en `dashboard`. `ServicioReportes.NormalizarRango` ya normaliza `desde/hasta`.
- `VentaPorUsuarioDto` agrupa por quien registró la venta (cajero que cobra), no por el mesonero que tomó el pedido. `Comanda` hereda `CreatedBy`, que el interceptor llena con el usuario autenticado.
- `ComandaDetalle` solo conserva `LastModifiedAt`, que cambia con cualquier edición, así que no sirve para medir el momento de servicio. Estados: Recibido, EnProceso, Preparado, Entregado, Cancelado.
- `SesionCaja` ya guarda `MontoEsperado`, `MontoContado` y `Descuadre`; `SesionMesa` guarda `AbiertaEn` y `CerradaEn`; `EstadoReserva` incluye `Asistio` y `NoAsistio`.
- QuestPDF 2026.9.1 está instalado en `Infrastructure` (`GeneradorMenuPdfQuestPdf`, registrado como singleton con licencia Community). No hay biblioteca de Excel.
- Frontend: `DashboardPage` (655 líneas) usa recharts con `ResponsiveContainer` y periodos fijos; `ReportesPage` (279 líneas) tiene pestañas ventas, inventario, compras y propinas con selector de fechas propio; `apiFetch` del cliente HTTP parsea JSON, por lo que no sirve para descargar archivos. El grupo `analitica` tiene `oculto: true` en `lib/navigation.tsx`.

Motivación y alcance: ver proposal.md. Comportamiento exigido: ver specs/.

## Goals / Non-Goals

**Goals:**
- Que cada KPI nuevo salga de una agregación en base de datos, sin cargar tablas completas en memoria.
- Que PDF y Excel salgan de un único modelo de datos, para que un reporte nuevo se exporte sin escribir dos generadores.
- Que la lógica nueva sea verificable con pruebas unitarias con Moq, sin base de datos.

**Non-Goals:**
- Reportes programados, envío por correo o plantillas personalizables por el usuario.
- Series históricas de existencias (la rotación usa la existencia actual).
- Metas o comisiones por mesonero.
- Cambios de diseño visual más allá de tamaños y orden de las gráficas.

## Decisions

**D1. Consultas nuevas en `ReportesRepository`, reglas en `ServicioReportes`.**
Cada KPI es un método del repositorio que devuelve filas agregadas por `GroupBy` en el servidor de base de datos; `ServicioReportes` calcula los derivados (porcentajes, tasas, orden, límites). Así los derivados se prueban con Moq del repositorio y las agregaciones con pruebas de integración sobre datos sembrados.
*Alternativa:* una vista materializada o tabla de resumen (más rápida, pero añade sincronización y no hace falta a esta escala).

**D2. Definiciones de los indicadores.**
- Ticket promedio: total USD / ventas completadas (excluye anuladas).
- Top productos: `DetalleVenta` de ventas no anuladas agrupado por variante; las cortesías suman unidades y no ingreso. No se descuentan devoluciones en esta versión (se documenta).
- Rotación: unidades vendidas del periodo / existencia actual; existencia 0 → "no calculable"; sin ventas y con existencia → "sin movimiento".
- Ocupación actual: mesas activas con cuenta abierta / mesas activas. No se calcula un porcentaje de ocupación sobre horas operativas porque el modelo no define el horario operativo por jornada.
- Permanencia: promedio de `CerradaEn - AbiertaEn` (las abiertas se cuentan hasta ahora y se marcan).
- No asistencia: `NoAsistio / (Asistio + NoAsistio)`; las canceladas se cuentan aparte.
- Descuadre: de `SesionCaja` cerradas del periodo, con resumen (total, promedio, mayor en valor absoluto).
- Mermas por motivo: reutiliza `MermasVsVentas` y agrega agrupación por motivo y costo estimado con `PrecioCompraUSD`.

**D3. Desempeño del mesonero por `Comanda.CreatedBy`.**
Se agrupa por el usuario creador de la comanda. Ventas generadas = suma de `SubtotalUSD` de ítems no cancelados y no cortesía. Cuentas atendidas = cuentas distintas con comandas del mesonero; ticket por cuenta = ventas generadas / cuentas. Tasa de cancelación = ítems cancelados / ítems totales.
Si un mismo mesero y otro crean comandas en la misma cuenta, cada uno recibe sus propios ítems.

**D4. `ServidoEn` en `ComandaDetalle`.**
Nueva columna nullable asignada la primera vez que el ítem pasa a Preparado o Entregado (nunca se sobrescribe). El tiempo de servicio es `ServidoEn - CreatedAt`; los ítems sin la marca se ignoran, así que los datos previos no distorsionan el promedio. La asignación vive en `ComandaDetalle.CambiarEstado` (dominio), con prueba unitaria.
*Alternativa:* reconstruir los tiempos desde la bitácora de `auditoria-total` (acopla dos funcionalidades y depende de que esa bitácora esté desplegada).

**D5. Modelo neutral `TablaReporte` y exportadores como puertos.**
Cada reporte exportable tiene un mapeador que produce `TablaReporte` (título, periodo y filtros, columnas con tipo y formato — texto, entero, decimal, moneda, porcentaje, fecha —, filas y fila de totales opcional). El puerto `IExportadorReportes` tiene dos implementaciones: `ExportadorReportesPdf` (QuestPDF, A4, horizontal si hay más de 6 columnas, encabezado repetido por página, pie con numeración) y `ExportadorReportesExcel` (ClosedXML, celdas numéricas y de fecha con formato, encabezado congelado y filtro automático, ancho ajustado). Los mapeadores y exportadores no tocan la base de datos.
*Alternativas:* un generador por reporte y formato (10 × 2 clases, duplicación); CSV (sencillo, pero el docente pidió PDF y Excel). ClosedXML se elige sobre EPPlus por su licencia permisiva; se fija la versión estable vigente al implementar.

**D5b. Orquestación de la exportación.**
`ServicioExportacionReportes` recibe el identificador del reporte, el formato y los filtros, obtiene los datos vía `IServicioReportes`, los pasa por el mapeador y entrega bytes con nombre y tipo de contenido. Para la bitácora de auditoría usa `IServicioAuditoria` con sus filtros. Valida el límite (5 000 filas PDF, 100 000 Excel) antes de renderizar.

**D6. Endpoints.**
`GET /api/v1/reportes/productos`, `/mesoneros`, `/caja`, `/ocupacion`, `/reservas-asistencia` para los KPIs nuevos, y `GET /api/v1/reportes/{reporte}/exportar?formato=pdf|xlsx&desde&hasta&...` para exportar. Cada ruta hereda la política de su consulta; `mesoneros` exige `Roles = "Admin"`. Un periodo inválido responde 400 mediante una validación común (`desde <= hasta`, máximo 366 días, 30 días por defecto).

**D7. Descarga autenticada en el cliente.**
`packages/api-client` añade `apiDownload(path, query)` que hace `fetch` con el token y devuelve un `Blob` y el nombre desde `Content-Disposition`; la interfaz crea un enlace temporal para guardar el archivo. Se descarta un enlace directo porque el JWT viaja en el encabezado `Authorization`, no en cookie.

**D8. Dashboard con periodo.**
Un componente `SelectorPeriodo` (hoy, 7 días, 30 días, mes, personalizado) fija `desde/hasta` en el estado de la URL y alimenta todas las consultas del Dashboard y de Reportes, de modo que se comparte entre ambas páginas. Las claves de TanStack Query incluyen el periodo. Las gráficas usan un contenedor de altura fija por tamaño (`h-64` en escritorio, `h-52` en móvil) y esqueleto de carga. Los KPIs nuevos se agrupan en tarjetas con los componentes existentes de `@licoreria/ui`.

**D9. Navegación.**
Se quita `oculto` del grupo `analitica` y del ítem `/reportes`. El ítem `/auditoria` lo reactiva `auditoria-total`; el cambio que se implemente segundo no debe volver a ocultarlo. La pestaña de mesoneros se muestra solo si `esAdmin`.

## Risks / Trade-offs

- [Consultas agregadas lentas con muchas ventas] → Agrupar en el servidor, filtrar por rango con los índices existentes de `Venta.Fecha`, medir con un conjunto de datos de 50 000 ventas y añadir índices solo si hace falta.
- [Rotación basada en la existencia actual puede engañar si el stock acaba de reponerse] → Se documenta la definición en la interfaz (tooltip) y en el reporte; la serie histórica queda fuera de alcance.
- [Atribuir al `CreatedBy` falla si una comanda la crea un usuario compartido o el sistema] → Las comandas sin creador se agrupan como "Sin asignar" y no desaparecen del total.
- [PDF muy largo en memoria] → Límite de 5 000 filas con 422 explícito; Excel hasta 100 000 filas con escritura por filas.
- [Nueva dependencia (ClosedXML)] → Un único paquete en `Infrastructure`, detrás de un puerto; se puede reemplazar sin tocar la aplicación.
- [Conflicto de migraciones con `concurrencia-stock` (`Reservado`) y `auditoria-total` (`tipo`)] → Las tres añaden columnas independientes; se aplican en orden de implementación y se regenera la migración si hay choque en el snapshot.

## Migration Plan

1. Aplicar la migración de `ServidoEn` (columna nullable, sin relleno).
2. Desplegar backend y frontend juntos: el frontend nuevo consulta endpoints que el backend antiguo no tiene.
3. Rollback: revertir la migración elimina la columna; el resto no cambia el esquema.
