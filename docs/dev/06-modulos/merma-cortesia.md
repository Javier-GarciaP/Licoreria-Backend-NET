# Módulo · Merma y Cortesía

Caso de negocio central del local: bebidas que se **dañan o se parten** no se venden,
pero deben descontarse del inventario, y a veces se reponen **sin cobro** a la mesa.

## Escenario

> A una mesa se le salieron **dos cervezas dañadas**. Esas dos se descuentan del
> inventario y se le entregan **dos más sin cobro**, que también se descuentan.

## Modelo

- `movimiento_inventario` con tipo **Merma** → baja por el daño.
- `movimiento_inventario` con tipo **Cortesía** → baja por la reposición sin cobro.
- `merma` detalla el motivo (`danado`, `partido`, `vencido`) y si hubo reposición.
- `comanda_detalle` con `es_cortesia = true` → línea a precio cero en la cuenta.

## Flujo

```mermaid
sequenceDiagram
    participant M as Mesero
    participant API as API
    participant INV as Inventario
    participant CU as Cuenta

    M->>API: Reporta 2 cervezas dañadas (mesa X)
    API->>INV: Movimiento Merma -2 (motivo danado)
    API->>CU: Registra la incidencia
    M->>API: Repone 2 cervezas sin cobro
    API->>INV: Movimiento Cortesia -2
    API->>CU: Comanda detalle cortesia (precio 0)
    Note over INV: Inventario descontado 4 unidades
    Note over CU: La mesa no paga la reposicion
```

## Reglas

1. La merma **siempre** descuenta inventario.
2. La reposición sin cobro genera cortesía (también descuenta) y **no** se cobra.
3. El reporte de pérdidas agrupa por motivo, producto y mesero.
4. El administrador puede exigir autorización para mermas sobre un umbral (previsto).

## Endpoints

| Método | Ruta | Descripción |
| :--- | :--- | :--- |
| `POST` | `/api/v1/mermas` | Registra una merma y su posible reposición. |
| `GET` | `/api/v1/mermas` | Lista con filtros (motivo, fecha, mesero). |
| `GET` | `/api/v1/reportes/mermas` | Reporte de pérdidas. |

## KPIs asociados

- Unidades y costo total de mermas por período.
- Porcentaje de merma sobre ventas.
- Mermas por motivo y por responsable.
