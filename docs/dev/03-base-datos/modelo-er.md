# Modelo Entidad-Relación

El modelo se organiza por **esquemas** (bounded contexts) en PostgreSQL. El sistema es
de **una única sucursal** (ver [ADR 0007](../adr/0007-sucursal-unica.md)).

## Vista general por esquemas

```mermaid
flowchart LR
    SEC[security] --> SAL[sales]
    SEC --> INV[inventory]
    CAT[catalog] --> INV
    CAT --> SAL
    INV --> PUR[purchasing]
    SAL --> CASH[cash]
    CLUB[club] --> SAL
    CRM[crm] --> SAL
    FIN[finance] --> SAL
    CONT[content] --> CLUB
    AI[ai] --> CONT
```

| Esquema | Propósito |
| :--- | :--- |
| `catalog` | Productos, variantes, precios, marcas, impuestos y recetas. |
| `inventory` | Existencias y movimientos (kardex), mermas y cortesías. |
| `purchasing` | Proveedores, órdenes de compra y cuentas por pagar. |
| `sales` | Ventas, comandas, cuentas, abonos, pagos y facturación. |
| `cash` | Sesiones de caja, movimientos y arqueos. |
| `club` | Zonas, plano, mesas, reservas y eventos. |
| `crm` | Clientes, fidelidad y cuentas por cobrar. |
| `finance` | Monedas, tasas de cambio y tesorería. |
| `security` | Usuarios, roles, permisos y auditoría. |
| `content` | Contenido de la web pública y menú digital. |
| `ai` | Trabajos y resultados del módulo de IA. |

## `catalog`

```mermaid
erDiagram
    CATEGORIA ||--o{ PRODUCTO : clasifica
    MARCA ||--o{ PRODUCTO : agrupa
    UNIDAD_MEDIDA ||--o{ PRODUCTO : mide
    IMPUESTO ||--o{ PRODUCTO : grava
    PRODUCTO ||--o{ PRODUCTO_VARIANTE : presenta
    PRODUCTO_VARIANTE ||--o{ CODIGO_BARRAS : identifica
    LISTA_PRECIO ||--o{ PRECIO_PRODUCTO : define
    PRODUCTO_VARIANTE ||--o{ PRECIO_PRODUCTO : tiene
    PRODUCTO ||--o{ RECETA : compone
    PRODUCTO_VARIANTE ||--o{ RECETA : consume
```

## `inventory`

```mermaid
erDiagram
    PRODUCTO_VARIANTE ||--o{ STOCK_PRODUCTO : existe
    TIPO_MOVIMIENTO ||--o{ MOVIMIENTO_INVENTARIO : clasifica
    PRODUCTO_VARIANTE ||--o{ MOVIMIENTO_INVENTARIO : afecta
    MOVIMIENTO_INVENTARIO ||--o| MERMA : detalla
```

## `club`

```mermaid
erDiagram
    ZONA ||--o{ MESA : contiene
    PLANO ||--o{ PLANO_ELEMENTO : dibuja
    ZONA ||--o{ PLANO_ELEMENTO : ubica
    MESA ||--o{ RESERVA_MESA : reservada
    RESERVA ||--o{ RESERVA_MESA : incluye
    RESERVA ||--o{ PEDIDO_ANTICIPADO : anticipa
    RESERVA ||--o{ RESERVA_PAGO : asegura
    EVENTO ||--o{ EVENTO_MEDIA : publica
```

## `sales`

```mermaid
erDiagram
    SESION_MESA ||--|| CUENTA : posee
    CUENTA ||--o{ COMANDA : agrupa
    COMANDA ||--o{ COMANDA_DETALLE : contiene
    CUENTA ||--o{ ABONO : recibe
    CUENTA ||--o| VENTA : cierra
    VENTA ||--o{ DETALLE_VENTA : contiene
    VENTA ||--o{ PAGO : liquida
    METODO_PAGO ||--o{ PAGO : tipo
    VENTA ||--o| COMPROBANTE_FISCAL : emite
```

## `cash` / `finance` / `security` / `content` / `ai`

```mermaid
erDiagram
    SESION_CAJA ||--o{ MOVIMIENTO_CAJA : registra
    SESION_CAJA ||--o{ ARQUEO_DENOMINACION : cuenta
    DENOMINACION ||--o{ ARQUEO_DENOMINACION : valor
    MONEDA ||--o{ TASA_CAMBIO : cotiza
    USUARIO ||--o{ USUARIO_ROL : tiene
    ROL ||--o{ USUARIO_ROL : asigna
    ROL ||--o{ ROL_PERMISO : concede
    PERMISO ||--o{ ROL_PERMISO : otorga
    USUARIO ||--o{ AUDIT_LOG : genera
    PLANTILLA_UI ||--o{ AI_GENERACION : usa
```

> El detalle de columnas, tipos y restricciones está en
> [`diccionario-datos.md`](diccionario-datos.md) y [`esquemas.md`](esquemas.md).
