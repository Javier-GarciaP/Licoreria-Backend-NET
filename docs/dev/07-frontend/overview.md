# Frontend

Dos aplicaciones **React + Vite + TypeScript + Tailwind CSS** en un monorepo de
workspaces (ver [ADR 0003](../adr/0003-frontend-react-tailwind.md)).

## Estructura

```
frontend/
├── apps/
│   ├── public-web/     # Web publica (clientes)
│   └── admin/          # Sistema interno (staff)
└── packages/
    ├── ui/             # Design system (Tailwind)
    ├── api-client/     # Cliente generado desde openapi/
    ├── types/          # Tipos compartidos
    └── config/         # ESLint / TS / Tailwind compartidos
```

## App pública (`public-web`)

| Sección | Descripción |
| :--- | :--- |
| Inicio | Presentación del local y accesos rápidos. |
| Catálogo | Productos y precios (USD/BS). |
| Menú | Comida y bebida. |
| Tasas del día | Tasa de cambio vigente. |
| Reservas | Selección de zona/mesa en plano y seña. |
| Eventos | Próximos eventos. |
| Contacto | WhatsApp, horarios y ubicación. |

## App interna (`admin`)

| Sección | Descripción |
| :--- | :--- |
| Dashboard | KPIs del negocio. |
| Catálogo / Inventario | Productos, precios, stock, mermas. |
| Mesas / Plano | Editor de distribución del local. |
| Reservas | Gestión y validación de señas. |
| Operación | Comandas, KDS barra y cocina, vista mesero. |
| Caja / POS | Sesiones de caja, abonos y cobro. |
| Compras | Proveedores y órdenes. |
| CRM | Clientes y fidelidad. |
| Contenido | Páginas, eventos y menú digital. |
| Administración | Usuarios, roles y configuración. |

## Stack y librerías

| Necesidad | Herramienta |
| :--- | :--- |
| Datos del servidor | TanStack Query (React Query) |
| Estado global | Zustand |
| Ruteo | React Router |
| Editor de plano | dnd-kit |
| Formularios | React Hook Form + Zod |
| Gráficas | Recharts |
| Tiempo real | Cliente SignalR |
| QR / PDF | `qrcode` y generador de PDF |

## Integración con la API

- El cliente se **genera desde `openapi/licoreria.yaml`** (`packages/api-client`).
- La autenticación usa JWT (ver [seguridad](../05-seguridad/jwt.md)).
- Las comandas y mesas se actualizan por **SignalR**.

## Design system

- `packages/ui` centraliza componentes y **design tokens**.
- Tailwind con configuración compartida en `packages/config`.
- Los mismos tokens alimentan la **UI generativa** del módulo de IA.
