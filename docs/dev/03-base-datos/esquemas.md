# Esquemas y Tablas

Listado de tablas por esquema. Todas las entidades de negocio heredan los campos de
auditoría: `id` (uuid), `created_at`, `last_modified_at`, `is_deleted`, `row_version`.

## `catalog`

| Tabla | Descripción |
| :--- | :--- |
| `marca` | Marcas de productos. |
| `categoria` | Categorías jerárquicas (`categoria_padre_id`). |
| `unidad_medida` | Unidades (unidad, botella, tobo, plato). |
| `impuesto` | Impuestos aplicables (IVA, IGTF). |
| `producto` | Producto base (nombre, marca, categoría, tipo, grado alcohólico). |
| `producto_variante` | Presentación vendible; concentra stock y precio. |
| `codigo_barras` | Códigos de barras por variante (varios por variante). |
| `lista_precio` | Listas (detal, mayorista, happy hour). |
| `precio_producto` | Precio de una variante en una lista y moneda. |
| `receta` | Insumos que consume una variante preparada. |
| `modificador` | Extras y modificadores de productos. |

## `inventory`

| Tabla | Descripción |
| :--- | :--- |
| `stock_producto` | Existencia actual por variante (global, una sucursal). |
| `tipo_movimiento` | Compra, venta, ajuste, merma, cortesía, consumo interno. |
| `movimiento_inventario` | Kardex inmutable con cantidad, costo y referencia. |
| `merma` | Detalle de una merma (motivo: dañado, partido, vencido). |
| `lote` | Lotes y fechas de vencimiento. |
| `ajuste_inventario` | Cabecera de ajustes. |
| `toma_fisica` | Conteos físicos de inventario. |

## `purchasing`

| Tabla | Descripción |
| :--- | :--- |
| `proveedor` | Proveedores con RIF y datos de contacto. |
| `orden_compra` | Órdenes de compra y su estado. |
| `orden_compra_detalle` | Líneas de la orden. |
| `recepcion` | Recepción de mercancía (actualiza inventario y costos). |
| `cuenta_por_pagar` | Cuentas por pagar a proveedores. |
| `pago_proveedor` | Pagos aplicados a cuentas por pagar. |

## `sales`

| Tabla | Descripción |
| :--- | :--- |
| `metodo_pago` | Efectivo USD, PagoMóvil, Zelle, punto, crédito. |
| `sesion_mesa` | Ocupación de una mesa (apertura/cierre). |
| `cuenta` | Consumos acumulados, abonos y saldo. |
| `comanda` | Pedido enviado a un área (barra/cocina). |
| `comanda_detalle` | Línea con estado y área destino. |
| `abono` | Pagos parciales sobre una cuenta. |
| `venta` | Venta cerrada con totales USD/BS y tasa. |
| `detalle_venta` | Líneas de la venta. |
| `pago` | Pagos de la venta (permite pago mixto). |
| `devolucion` | Devoluciones y notas de crédito. |
| `promocion` | Promociones y descuentos. |
| `comprobante_fiscal` | Número de factura y número de control. |
| `libro_venta` | Registro fiscal de ventas. |

## `cash`

| Tabla | Descripción |
| :--- | :--- |
| `denominacion` | Billetes y monedas para el arqueo. |
| `sesion_caja` | Apertura, cierre y turno de caja. |
| `movimiento_caja` | Ingresos y egresos de caja. |
| `arqueo_denominacion` | Conteo por denominación al cierre. |

## `club`

| Tabla | Descripción |
| :--- | :--- |
| `zona` | Áreas del local (barra, mesas, juegos, pista, VIP). |
| `plano` | Versión del plano del local. |
| `plano_elemento` | Elementos del plano (barra, pista, baños, mesas). |
| `mesa` | Mesa con capacidad, forma y posición. |
| `silla` | Sillas asociadas a una mesa (opcional). |
| `reserva` | Reserva de una fecha/hora. |
| `reserva_mesa` | Mesas incluidas en una reserva. |
| `pedido_anticipado` | Ítems pedidos al reservar. |
| `reserva_pago` | Seña con comprobante y estado de validación. |
| `evento` | Eventos del local. |
| `evento_media` | Imágenes y media de un evento. |
| `lista_vip` | Clientes VIP. |
| `entrada` | Control de acceso / cover con QR. |

## `crm`

| Tabla | Descripción |
| :--- | :--- |
| `cliente` | Datos del cliente y fiscales (RIF/CI). |
| `puntos_movimiento` | Movimientos de puntos de fidelidad. |
| `cuenta_por_cobrar` | Crédito otorgado a clientes. |

## `finance`

| Tabla | Descripción |
| :--- | :--- |
| `moneda` | USD, BS. |
| `tasa_cambio` | Tasa diaria (BCV/paralelo) por fecha. |
| `cuenta_bancaria` | Cuentas del local. |
| `movimiento_tesoreria` | Movimientos de tesorería. |
| `cierre_caja` | Consolidado de cierre (Z). |

## `security`

| Tabla | Descripción |
| :--- | :--- |
| `usuario` | Usuarios del sistema. |
| `rol` | Roles (administrador, cajero, mesero, barra, cocina, host). |
| `permiso` | Permisos atómicos. |
| `rol_permiso` | Relación rol-permiso. |
| `usuario_rol` | Relación usuario-rol. |
| `audit_log` | Auditoría de acciones sensibles. |

## `content`

| Tabla | Descripción |
| :--- | :--- |
| `pagina` | Páginas de la web pública. |
| `seccion` | Secciones dentro de una página. |
| `bloque_contenido` | Bloques de contenido (texto, galería, CTA). |
| `plantilla_ui` | Plantillas y design tokens. |
| `media_asset` | Imágenes y archivos. |
| `horario_atencion` | Días y horarios de atención. |
| `local_info` | Información del local. |
| `menu_digital` | Menú digital y sus secciones. |
| `qr_code` | Códigos QR generados. |

## `ai`

| Tabla | Descripción |
| :--- | :--- |
| `ai_generacion` | Trabajo de IA con entrada, salida, modelo y costo. |
| `plano_generado` | Resultado de digitalizar un plano desde papel. |
