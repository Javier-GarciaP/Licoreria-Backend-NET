# Diccionario de Datos

Detalle de columnas de las tablas principales. El resto sigue las mismas convenciones
de nombres y tipos descritas en [`postgresql.md`](postgresql.md).

## Campos comunes (herencia de `BaseEntity`)

| Columna | Tipo | Restricciones | Descripción |
| :--- | :--- | :--- | :--- |
| `id` | `uuid` | PK, default `gen_random_uuid()` | Identificador único. |
| `created_at` | `timestamptz` | NOT NULL | Fecha de creación (UTC). |
| `last_modified_at` | `timestamptz` | NULL | Última modificación (UTC). |
| `is_deleted` | `boolean` | NOT NULL, default `false` | Borrado lógico. |
| `row_version` | `xmin` / `bigint` | — | Concurrencia optimista. |
| `created_by` | `uuid` | NULL, FK `security.usuario` | Usuario creador. |
| `updated_by` | `uuid` | NULL, FK `security.usuario` | Usuario que modificó. |

## `catalog.producto_variante`

| Columna | Tipo | Restricciones | Descripción |
| :--- | :--- | :--- | :--- |
| `id` | `uuid` | PK | Identificador. |
| `producto_id` | `uuid` | FK `producto`, NOT NULL | Producto base. |
| `nombre` | `varchar(120)` | NOT NULL | Nombre de la presentación. |
| `sku` | `varchar(50)` | UNIQUE | Código interno. |
| `unidad_medida_id` | `uuid` | FK `unidad_medida` | Unidad de venta. |
| `precio_compra` | `numeric(18,2)` | NOT NULL | Costo de referencia. |
| `activo` | `boolean` | NOT NULL, default `true` | Disponible para venta. |

## `inventory.stock_producto`

| Columna | Tipo | Restricciones | Descripción |
| :--- | :--- | :--- | :--- |
| `variante_id` | `uuid` | PK, FK `catalog.producto_variante` | Variante. |
| `cantidad` | `numeric(14,3)` | NOT NULL, default 0 | Existencia actual. |
| `cantidad_reservada` | `numeric(14,3)` | NOT NULL, default 0 | Comprometida. |
| `stock_minimo` | `numeric(14,3)` | NOT NULL, default 0 | Umbral de alerta. |

## `inventory.movimiento_inventario`

| Columna | Tipo | Restricciones | Descripción |
| :--- | :--- | :--- | :--- |
| `id` | `uuid` | PK | Identificador. |
| `variante_id` | `uuid` | FK, NOT NULL | Variante afectada. |
| `tipo_movimiento_id` | `uuid` | FK, NOT NULL | Tipo de movimiento. |
| `cantidad` | `numeric(14,3)` | NOT NULL | Positiva (entrada) o negativa (salida). |
| `costo_unitario` | `numeric(18,2)` | NULL | Costo al momento. |
| `referencia_tipo` | `varchar(40)` | NULL | Origen (`venta`, `compra`, `merma`...). |
| `referencia_id` | `uuid` | NULL | Id del documento origen. |
| `motivo` | `varchar(200)` | NULL | Descripción del movimiento. |

## `inventory.merma`

| Columna | Tipo | Restricciones | Descripción |
| :--- | :--- | :--- | :--- |
| `id` | `uuid` | PK | Identificador. |
| `movimiento_id` | `uuid` | FK `movimiento_inventario`, UNIQUE | Movimiento de baja. |
| `motivo` | `varchar(20)` | NOT NULL | `danado`, `partido`, `vencido`. |
| `repuesto` | `boolean` | NOT NULL, default `false` | Si se repuso sin cobro. |
| `comanda_detalle_id` | `uuid` | NULL | Línea de cortesía asociada. |

## `sales.cuenta`

| Columna | Tipo | Restricciones | Descripción |
| :--- | :--- | :--- | :--- |
| `id` | `uuid` | PK | Identificador. |
| `sesion_mesa_id` | `uuid` | FK `sesion_mesa`, UNIQUE | Sesión de mesa. |
| `estado` | `varchar(20)` | NOT NULL | `abierta`, `por_cobrar`, `cerrada`. |
| `total` | `numeric(18,2)` | NOT NULL, default 0 | Consumo acumulado USD. |
| `total_abonado` | `numeric(18,2)` | NOT NULL, default 0 | Abonos USD. |
| `saldo` | `numeric(18,2)` | GENERATED | `total - total_abonado`. |

## `sales.comanda_detalle`

| Columna | Tipo | Restricciones | Descripción |
| :--- | :--- | :--- | :--- |
| `id` | `uuid` | PK | Identificador. |
| `comanda_id` | `uuid` | FK `comanda`, NOT NULL | Comanda. |
| `variante_id` | `uuid` | FK `catalog.producto_variante` | Producto. |
| `cantidad` | `numeric(14,3)` | NOT NULL | Cantidad. |
| `precio_unitario` | `numeric(18,2)` | NOT NULL | Precio al momento. |
| `area_destino` | `varchar(10)` | NOT NULL | `barra` o `cocina`. |
| `estado` | `varchar(15)` | NOT NULL | `recibido`, `preparado`, `entregado`, `cancelado`. |
| `es_cortesia` | `boolean` | NOT NULL, default `false` | Línea sin cobro. |

## `sales.venta` / `sales.pago`

| Tabla.Columna | Tipo | Descripción |
| :--- | :--- | :--- |
| `venta.tasa_cambio` | `numeric(18,4)` | Tasa aplicada. |
| `venta.total_usd` | `numeric(18,2)` | Total en dólares. |
| `venta.total_bs` | `numeric(18,2)` | Total en bolívares. |
| `pago.metodo_pago_id` | `uuid` | FK `metodo_pago`. |
| `pago.monto` | `numeric(18,2)` | Monto del pago. |
| `pago.moneda_id` | `uuid` | FK `finance.moneda`. |
| `pago.propina` | `numeric(18,2)` | Propina (opcional). |

## `club.reserva`

| Columna | Tipo | Restricciones | Descripción |
| :--- | :--- | :--- | :--- |
| `id` | `uuid` | PK | Identificador. |
| `cliente_id` | `uuid` | FK `crm.cliente`, NULL | Cliente. |
| `fecha_hora` | `timestamptz` | NOT NULL | Fecha y hora. |
| `personas` | `integer` | NOT NULL | Número de personas. |
| `estado` | `varchar(20)` | NOT NULL | `pendiente`, `confirmada`, `cancelada`, `asistio`. |
| `origen` | `varchar(20)` | NOT NULL | `web`, `whatsapp`, `presencial`. |

## `ai.ai_generacion`

| Columna | Tipo | Descripción |
| :--- | :--- | :--- |
| `id` | `uuid` | Identificador. |
| `tipo` | `varchar(40)` | `plano`, `seccion_web`, `imagen`, `pronostico`. |
| `estado` | `varchar(20)` | `pendiente`, `procesando`, `completado`, `error`. |
| `input` | `jsonb` | Entrada del trabajo. |
| `output` | `jsonb` | Resultado estructurado. |
| `modelo` | `varchar(80)` | Modelo utilizado. |
| `costo` | `numeric(10,4)` | Costo estimado. |
| `aprobado` | `boolean` | Requiere aprobación humana antes de publicar. |
