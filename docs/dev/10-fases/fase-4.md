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
    └── ui/            design system (paleta lavanda/oscuro actual; ver "Tema")
```

## Decisiones clave

- **Context API** para estado global: `AuthContext` (JWT en `localStorage`),
  `ThemeContext` (Claro / Oscuro, persistente) y `ModoContext`
  (Modo Licorería / Modo Discoteca; `ModoProvider` montado en `main.tsx`).
- **Tema (decisión registrada):** se conserva la paleta **lavanda/álamo** actual del
  design system (`frontend/DESIGN.md`). El "Azul UNET" (`#003366`) de la especificación
  original **no se adopta**: `DESIGN.md` §7 lo prohíbe como color de acento y el cambio
  rompería la deuda visual ya cerrada. Cumplimiento: *desviación aprobada*.
- **TanStack Query** para datos de servidor, **React Hook Form + Zod** en formularios,
  **Recharts** para gráficas, **SignalR** para tiempo real y **sonner** para toasts.
- **Accesibilidad básica:** modales con `role="dialog"`/`aria-modal`, cierre con `Escape`,
  foco gestionado, `scope="col"` en tablas y enlace "saltar al contenido".
- **RBAC**: la API valida en servidor; el front oculta rutas/acciones con
  `ProtectedRoute` y `Can`.

## Requerimientos del Grupo 5 cubiertos

| Requerimiento | Implementación |
| :--- | :--- |
| Modo dual Licorería / Discoteca | `ModoProvider` en `main.tsx` + botón en `AppShell`; `filtrarGrupos`/`puedeAcceder` filtran por `modos` (Licorería oculta Salón/`/plano`; Discoteca = nav completa; Dashboard en ambos) |
| Plano interactivo | `PlanoPage` (SVG) con estados Libre/Reservada/Ocupada/En limpieza |
| KDS Barra | `KdsPage` con SignalR `/hubs/comandas` (área `barra`) |
| Máquina de estados de comandas | `MaquinaEstadosComanda` inyectada en `ServicioCuentas.CambiarEstadoItemAsync` (Recibido → EnProceso → Preparado → Entregado); estado **Entregado** se marca desde `PosCarta` (mesonero) |
| Conflictos de reservas VIP | `ReservasPage` con creación, señas y pedidos |
| Mermas y cortesías | `MermasPage` |
| Pagos mixtos USD/Bs | Abonos y cierre en `CuentaPage`/`PosCarta` y pago único en `PosPage` con selector USD/Bs; el backend convierte Bs → USD con la tasa Paralelo vigente (`ServicioCuentas` y `ServicioVentas`) |
| Dashboard analítico | `DashboardPage` con `overflow-x-auto` (heatmap con `min-w` y bento KPI), salud de inventario y mermas vs ventas |
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

## Web pública · tema "Hungry Tiger"

`apps/public-web` no usa el tema operativo: es un póster tipográfico *spice-label*
(paleta dorado-sobre-óxido, tipografía display `Antonio`) con **vitrina 3D** de la
botella de vino (glTF renderizado con React Three Fiber, cargado de forma diferida) y
**reserva web con seña** (selección de mesas y pago).
Detalle de tokens en [`frontend/DESIGN.md`](../../../frontend/DESIGN.md) §8.

## Administración (CRUD)

Además de la operación, el panel interno incorpora gestión completa de:

- **Catálogo:** productos con variantes (crear/editar/eliminar), categorías y marcas.
- **Usuarios:** alta/edición, cambio de contraseña, revocación de sesiones y baja lógica.
- **Clientes (CRM):** CRUD y acumular/canjear puntos.
- **Caja:** apertura, movimientos, arqueo/cierre (Z) e historial de sesiones.
- **Inventario:** existencias con filtros de mínimo, kardex filtrable, ajustes, lotes con
  vencimientos y tomas físicas con diferencias; registro e historial de mermas.
- **Compras:** proveedores (CRUD), órdenes de compra con ciclo de aprobación, recepciones
  que actualizan inventario y cuentas por pagar con registro de pagos.
- **Contenido:** páginas con secciones y bloques, eventos, horarios de atención,
  información del local, menú digital con QR y subida de archivos.
- **Finanzas:** registro e histórico de tasas de cambio.
- **Salón:** editor de planos con arrastre libre (`dnd-kit`) y CRUD de zonas y mesas.

Pendiente para siguientes iteraciones: el módulo de IA (generaciones y aprobación).

## Pruebas

- **Backend:** xUnit + Moq: `ServicioCatalogoProductoTests`, `ServicioClubReservasTests`,
  `ServicioVentasTests` (pagos mixtos USD/Bs con tasa) y `ServicioCuentasTests`
  (abonos con conversión y máquina de estados), más pruebas de integración.
- **Frontend:** Vitest + React Testing Library + MSW
  (`npm run test` en `frontend/`); `navigation.test.ts` cubre el filtrado por modo.
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
