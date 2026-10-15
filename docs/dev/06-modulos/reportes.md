# Módulo · Reportes, KPIs y exportación

Convierte la operación en indicadores para decidir y permite descargar cada reporte en PDF o Excel.

## Periodo de los reportes

Todas las consultas con periodo aceptan `desde` y `hasta`:

- Sin ninguno, usan los **últimos 30 días**.
- Una fecha sin hora (`2026-10-13`) se interpreta en UTC y el día final se incluye completo.
- `desde` posterior a `hasta`, o un periodo de más de **366 días**, responden `400`.

En la interfaz, el **selector de periodo** (hoy, últimos 7 días, últimos 30 días, mes actual y
personalizado) vive en la URL (`?periodo=7d` o `?periodo=personalizado&desde=…&hasta=…`) y lo comparten el
Dashboard y Reportes. Un rango personalizado inválido no consulta y muestra el motivo.

## Definiciones de los indicadores

| Indicador | Cálculo |
| :--- | :--- |
| **Ticket promedio** | Total USD de las ventas completadas del periodo entre su cantidad. Las ventas anuladas no cuentan; sin ventas es `0`. |
| **Ventas por método de pago** | Pagos del periodo agrupados por método y moneda, con el monto en la moneda original y la cantidad de pagos. |
| **Top productos** | Líneas de venta de ventas no anuladas, por variante. Se ordena por `Unidades` o `Ingresos` (`limite` de 1 a 100, 10 por defecto). Las cortesías suman unidades pero no ingreso. No descuenta devoluciones en esta versión. |
| **Rotación** | Unidades vendidas del periodo entre la **existencia actual**. Con ventas y existencia `0` es *no calculable*; con existencia y sin ventas es *sin movimiento* (rotación `0`). No usa series históricas de existencias. |
| **Ocupación actual** | Mesas activas con una sesión abierta y cuenta sin cerrar, entre las mesas activas, en total y por zona. No se calcula un porcentaje sobre horas operativas porque el modelo no define el horario operativo de cada jornada. |
| **Permanencia y rotación de mesas** | Promedio de `cierre − apertura` de las sesiones iniciadas en el periodo; las sesiones aún abiertas cuentan hasta el momento de la consulta. Sesiones por mesa = sesiones entre mesas activas. Las sesiones sin mesa del plano se agrupan como «Sin zona». |
| **No asistencia** | `NoAsistio / (Asistio + NoAsistio)` sobre las reservas del periodo (por `FechaHora`). Las canceladas se cuentan aparte. Sin reservas que debieran concretarse es `null`. |
| **Descuadre de caja** | De los turnos **cerrados** en el periodo: descuadre = contado − esperado. Resume total, promedio y el mayor en valor absoluto. Un turno abierto aún no tiene arqueo y no entra. |
| **Mermas** | Mermas del periodo por motivo, con unidades, costo estimado (costo del movimiento o precio de compra) y su proporción frente a las ventas. |

## Desempeño del mesonero

Se atribuye al usuario que **creó la comanda** (`Comanda.CreatedBy`), aunque otra persona cobre la cuenta.
Las comandas sin creador aparecen como «Sin asignar» y no desaparecen del total.

| Indicador | Cálculo |
| :--- | :--- |
| Comandas / cuentas | Comandas y cuentas distintas con comandas del mesonero en el periodo. |
| Ítems servidos | Ítems en estado `Preparado` o `Entregado`. |
| Ventas generadas | Suma de cantidad × precio de los ítems no cancelados y no cortesía. |
| Ticket por cuenta | Ventas generadas entre las cuentas atendidas. |
| Tasa de cancelación | Ítems cancelados entre ítems totales. |
| Tiempo de servicio | Promedio de `ServidoEn − CreatedAt` de los ítems con esa marca. |

`ComandaDetalle.ServidoEn` guarda la **primera vez** que un ítem pasa a `Preparado` o `Entregado` y nunca se
sobrescribe. La migración la añade nullable y sin relleno: los ítems anteriores no tienen marca y no
distorsionan el promedio. El reporte solo lo ve el **Administrador** (`403` para otros roles).

## Endpoints

| Ruta | Acceso |
| :--- | :--- |
| `GET /api/v1/reportes/productos` | `sales:read` |
| `GET /api/v1/reportes/ocupacion` | `club:read` |
| `GET /api/v1/reportes/reservas-asistencia` | `club:read` |
| `GET /api/v1/reportes/caja` | `cash:close` |
| `GET /api/v1/reportes/mesoneros` | Administrador |
| `GET /api/v1/reportes/{reporte}/exportar` | El de su consulta |

## Exportación a PDF y Excel

`GET /api/v1/reportes/{reporte}/exportar?formato=pdf|xlsx&desde&hasta&…` con los mismos filtros de la
consulta. Reportes exportables: `ventas`, `productos`, `inventario`, `compras`, `propinas`, `mesoneros`,
`caja`, `ocupacion`, `mermas-vs-ventas` y `auditoria` (con los filtros `entidad`, `usuarioId`, `accion`,
`tipo`, `desde` y `hasta` de la bitácora). Un reporte desconocido responde `404`; un formato distinto de
`pdf` y `xlsx`, `400`; sin el permiso de la consulta, `403`.

- **Contenido:** nombre del local, título, periodo y filtros, fecha y hora de generación (UTC), usuario que
  lo generó y totales cuando corresponde.
- **Nombre del archivo:** `reporte-<id>-<desde>_<hasta>.<ext>` (por ejemplo
  `reporte-ventas-2026-10-01_2026-10-13.xlsx`); los reportes sin periodo usan la fecha de generación.
- **Límites:** el PDF admite hasta **5 000 filas** (`422` que sugiere acotar el periodo o usar Excel) y el
  Excel hasta **100 000**.
- **Excel (ClosedXML):** una hoja por tabla; importes, cantidades y fechas son valores con formato (se
  pueden ordenar y sumar), encabezado congelado y filtro automático.
- **PDF (QuestPDF):** A4, horizontal con más de 6 columnas, encabezado de página y de tabla repetidos y
  pie con numeración.
- **Descarga autenticada:** el JWT viaja en la cabecera `Authorization`, así que el cliente
  (`apiDownload`) pide el archivo y lo guarda con un enlace temporal; el nombre sale de
  `Content-Disposition`.

## Arquitectura

Cada reporte tiene un **mapeador** (`MapeadoresReporte`) que lo convierte en tablas neutrales
(`TablaReporte`); el puerto `IExportadorReportes` tiene dos implementaciones (`ExportadorReportesPdf` y
`ExportadorReportesExcel`). Un reporte nuevo se exporta escribiendo solo su mapeador.
`ServicioExportacionReportes` orquesta datos, límites y nombre del archivo.
