# Fase 4 · Frontend React + Docker

Esta fase implementa el frontend de la plataforma (apps `admin` y `public-web`)
sobre la API v1 .NET + PostgreSQL, además de la orquestación con Docker.

> Alcance y checklist de contribución: [`frontend/README.md`](../../../frontend/README.md).
> Patrón de diseño: [`frontend/DESIGN.md`](../../../frontend/DESIGN.md).

## Arquitectura

Monorepo de workspaces npm (ADR [0003](../adr/0003-frontend-react-tailwind.md)):

```
frontend/
├── apps/admin/        SPA interna (staff)
├── apps/public-web/   Web pública (clientes)
└── packages/
    ├── config/        tokens Tailwind + presets compartidos
    ├── types/         DTOs TypeScript espejo de la API
    ├── api-client/    fetch + JWT/refresh + RFC 7807
    └── ui/            design system (tema oscuro / Azul UNET)
```

## Decisiones clave

- **Context API** para estado global: `AuthContext` (JWT en `localStorage`),
  `ThemeContext` (Oscuro / Azul UNET, persistente) y `ModoContext`
  (Modo Licorería / Modo Discoteca).
- **TanStack Query** para datos de servidor, **React Hook Form + Zod** en formularios,
  **Recharts** para gráficas, **SignalR** para tiempo real y **sonner** para toasts.
- **Accesibilidad básica:** modales con `role="dialog"`/`aria-modal`, cierre con `Escape`,
  foco gestionado, `scope="col"` en tablas y enlace "saltar al contenido".
- **RBAC**: la API valida en servidor; el front oculta rutas/acciones con
  `ProtectedRoute` y `Can`.

## Requerimientos del Grupo 5 cubiertos

| Requerimiento | Implementación |
| :--- | :--- |
| Modo dual Licorería / Discoteca | `ModoContext` + selector en el layout |
| Plano interactivo | `PlanoPage` (SVG) con estados Libre/Reservada/Ocupada/En limpieza |
| KDS Barra | `KdsPage` con SignalR `/hubs/comandas` (área `barra`) |
| Máquina de estados de comandas | Recibido → Preparado → Entregado en `CuentaPage` |
| Conflictos de reservas VIP | `ReservasPage` con creación, señas y pedidos |
| Mermas y cortesías | `MermasPage` |
| Pagos mixtos USD/Bs | `PosPage` y `CuentaPage` (abonos y cierre) |
| Dashboard analítico | `DashboardPage` con `overflow-x-auto`, heatmap, salud de inventario y mermas vs ventas |
| RBAC Guards | `ProtectedRoute` y `Can` |
| RFC 7807 | parser en `packages/api-client` + toasts |
| Mobile First / Skeletons / paginación servidor / validación / Docker | aplicados en toda la app |

## Desalojo de mesa en tiempo real

`POST /api/v1/mesas/{id}/desalojar` cierra la sesión de mesa (deja la cuenta en
`PorCobrar` si tiene saldo) y emite el evento SignalR **`mesa:actualizada`**
(`{ mesaId, estado, cuentaId }`). Los clientes que se unen al área `meseros`/`mesas`
refrescan el plano, de modo que la mesa queda **Libre** para el resto del personal.

## Backend añadido para esta fase

- `GET /api/v1/reportes/heatmap` — consumo por día de la semana y hora.
- `GET /api/v1/reportes/inventario-salud` — clasificación con mínimos/máximos.
- `GET /api/v1/reportes/mermas-vs-ventas` — comparativo valorizado.
- `POST /api/v1/mesas/{id}/desalojar` + evento `mesa:actualizada`.
- `MesaDto` enriquecido con `cuentaId` y `reservada`.

## Pruebas

- **Backend:** xUnit + Moq (`ServicioCatalogoProductoTests`) y pruebas de integración.
- **Frontend:** Vitest + React Testing Library + MSW
  (`npm run test` en `frontend/`).
- **CI:** `.github/workflows/frontend.yml` (lint, typecheck, tests y build).

## Orquestación

`docker compose up --build` levanta `postgres`, `api`, `admin`, `public-web` y `edge`
(nginx reverse proxy con soporte WebSocket para SignalR).

| Servicio | URL local |
| :--- | :--- |
| Admin | `http://localhost:4173` |
| Web pública | `http://localhost:4174` |
| API / Swagger | `http://localhost:5190` |
| Edge | `http://localhost:8090` |
