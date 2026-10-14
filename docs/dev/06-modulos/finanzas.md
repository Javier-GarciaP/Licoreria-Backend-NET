# Módulo · Finanzas y Tasas de Cambio

Administra las tasas de cambio (BCV/paralelo) usadas por el negocio.

## Entidades

- `tasa_cambio` (histórico BCV/paralelo por fecha)

## Reglas de negocio

1. Debe existir **una tasa vigente por fecha** y tipo (BCV/paralelo).
2. Las ventas guardan la tasa aplicada en el momento.
3. Los montos se expresan en USD y su equivalente en BS.

## Tasa de cambio

```mermaid
flowchart LR
    F[Fecha] --> T[Tasa vigente]
    T --> W[Web publica: tasas del dia]
    T --> V[Venta: total_bs = total_usd * tasa]
```

## Endpoints

| Método | Ruta | Descripción |
| :--- | :--- | :--- |
| `GET` | `/api/v1/tasas-cambio` | Histórico de tasas. |
| `GET` | `/api/v1/tasas-cambio/actual` | Tasa vigente. |
| `POST` | `/api/v1/tasas-cambio` | Registra tasa del día. |
