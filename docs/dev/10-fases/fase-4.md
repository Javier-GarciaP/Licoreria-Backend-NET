# Fase 4 · Frontend React + Docker

Esta fase implementa el frontend de la plataforma (apps `admin` y `public-web`)
sobre la API v1 .NET + PostgreSQL, además de la orquestación con Docker.

> Alcance y checklist de contribución: [`frontend/README.md`](../../../frontend/README.md).
> Patrón de diseño: [`frontend/DESIGN.md`](../../../frontend/DESIGN.md).

## Tecnologías usadas

| Capa | Tecnología |
| :--- | :--- |
| Build / bundler | **Vite** (dev server con HMR, build con code-splitting) |
| UI | **React 18** + **TypeScript** + **Tailwind CSS** (tokens en `packages/config`) |
| Enrutamiento | **react-router-dom v6** (`createBrowserRouter`) |
| Estado de servidor | **TanStack Query** (`useQuery`/`useMutation` + invalidación) |
| Estado global | **Context API** (`AuthContext`, `ThemeContext`, `ModoContext`) |
| Formularios | **React Hook Form** + validación **Zod**/FluentValidation en servidor |
| HTTP | Wrapper propio de **fetch** en `packages/api-client` (JWT + refresh + RFC 7807) |
| Tiempo real | **SignalR** (`@microsoft/signalr`, hub `/hubs/comandas`) |
| Gráficas / UX | **Recharts**, **sonner** (toasts), **dnd-kit** (editor de planos), skeletons |
| 3D (web pública) | **React Three Fiber** + glTF (botella de vino) |
| Pruebas FE | **Vitest** + **React Testing Library** + **MSW** (42 tests) |
| Pruebas BE | **xUnit** + **Moq** (116 tests, aislamiento de capas) |
| Empaquetado | **npm workspaces** (monorepo), **Docker Compose** (6 servicios) |

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

## Cómo se implementó

La fase se cerró completando **5 brechas** detectadas en la revisión del núcleo,
sobre una base ya funcional (commit `917d604`, con 94 tests backend y 33 frontend
verdes):

1. **Modo dual real:** el `ModoContext` existía pero no filtraba nada. Se añadió el
   campo `modos` por item/grupo en `navigation.tsx` y los helpers `visiblePorModo`,
   `filtrarGrupos` y `puedeAcceder` (el chequeo de modo ocurre **antes** del bypass
   de admin). `ModoProvider` se montó en `main.tsx` (envolviendo `ErrorBoundary`,
   `Toaster` y `AuthProvider`), el botón de conmutación en el header de `AppShell`
   y `App.tsx` aterriza en `/pos` (licorería) o `/` (discoteca). 9 tests nuevos en
   `navigation.test.ts`.
2. **Dashboard sin desborde:** heatmap dentro de `overflow-x-auto` con
   `min-w-[560px]` interno y bento de KPIs con scroll horizontal en móvil.
3. **Máquina de estados conectada:** `MaquinaEstadosComanda` se reescribió sobre el
   enum `EstadoItemComanda` (Recibido → EnProceso/Preparado → Entregado; Cancelado
   terminal) e inyectó en `ServicioCuentas.CambiarEstadoItemAsync`, que valida
   transiciones (`ReglaNegocioException`), ignora no-ops y deriva el estado de la
   comanda (`CalcularEstadoComanda`) + notificación SignalR. El estado **Entregado**
   se marca desde `PosCarta` (mesonero), manteniendo el KDS en 3 columnas.
4. **Pagos mixtos USD/Bs:** el backend convierte Bs → USD con la tasa Paralelo
   vigente en abonos (`ServicioCuentas.RegistrarAbonoAsync`) y en ventas
   (`ServicioVentas`), validando `tasa > 0`; el abono persiste monto/moneda
   originales. El frontend añadió selector de moneda en el cobro y abonos de
   `CuentaPage`, en `PosCarta` y en el panel de orden de `PosPage`, con
   `formatBS`/`formatUSD` en el historial.
5. **`EvaluadorMerma` integrado:** `ServicioInventario.RegistrarMermaAsync` consume
   los movimientos sugeridos por el evaluador (Merma + Cortesía opcional) con
   paridad de comportamiento; el evaluador pasó a aceptar `decimal`.

La verificación de cierre fue siempre la misma batería: `dotnet build` (0 errores),
`dotnet test` (116), `npm run typecheck`, `npm run lint` y `npm test` (42), además de
`prettier --check` (la deuda de formato preexistente no se tocó). Los cambios se
documentaron en el checklist de este archivo, en `frontend/README.md` y en el
`CHANGELOG`, y se registró la **desviación de tema aprobada** (se conserva la paleta
lavanda; el azul UNET `#003366` no se adopta, `DESIGN.md` §7).

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

## Cuestionario de defensa (Fase 4)

Preguntas frecuentes con respuestas breves ancladas al proyecto:

### 1. Frontend SPA y componentización

Es una **SPA** con React 18 + Vite: no hay renderizado en servidor ni recargas de
página; toda la UI se hidrata en un único `index.html` y la navegación la maneja el
router en cliente. La UI se organiza por funcionalidad: `pages/` por pantalla,
`components/` compartidos y por dominio (`components/pos/`, `components/mapa/`…), con
un design system propio en `packages/ui`. La comunicación es props → hijos y
eventos → padre; el estado de servidor vive fuera del árbol (TanStack Query).

### 2. Estructura

Monorepo npm con **workspaces**: `apps/admin`, `apps/public-web`, `apps/servicio` y
`packages/` compartidos (`types` DTOs espejo de la API, `api-client` fetch+JWT,
`ui` design system, `auth`, `config` tokens Tailwind). El backend .NET respeta
**Onion/Clean Architecture**: `Domain` (entidades y reglas de negocio, sin
dependencias de frameworks), `Application` (casos de uso, DTOs, validadores),
`Infrastructure` (EF Core, repositorios) y `WebAPI` (controllers, SignalR, DI).

### 3. Vite y enrutamiento

**Vite** sirve en desarrollo con HMR instantáneo y en producción genera un build con
code-splitting por módulo. El enrutamiento es **react-router-dom v6**
(`createBrowserRouter`); las rutas protegidas se envuelven en `<ProtectedRoute>` y la
visibilidad por rol/modo la decide `ContenidoProtegido` en `App.tsx`, evitando
renderizar pantallas no autorizadas.

### 4. Modularidad y gestión de estado de componentes

Estado en tres niveles: **local** con `useState`/custom hooks (`usePos`, `useTurno`,
`useRealtime`); **global** con Context API (`AuthContext`, `ThemeContext`,
`ModoContext`) sin prop-drilling; y **de servidor** con TanStack Query. Los
componentes son mayormente presentacionales (reciben props) y los contenedores
conectan a Query/Context.

### 5. Consumo de APIs y estado

Todo pasa por `packages/api-client` (wrapper de `fetch`): parsea errores
**RFC 7807** a toasts, adjunta el JWT y centraliza la lógica. TanStack Query gestiona
el estado remoto: `useQuery` para lecturas (caché, refresco, reintentos) y
`useMutation` + `invalidateQueries` para escrituras, eliminando `useEffect`
manuales y peticiones duplicadas.

### 6. Integración de peticiones HTTP asíncronas

Llamadas `async/await` con cancelación vía `AbortController`/signal de React Query al
desmontar; los estados de carga/error se renderizan con skeletons y toasts sin
bloquear la UI. En tiempo real se complementa con **SignalR** (`/hubs/comandas`)
para eventos push (comandas, mesas, turnos) en lugar de polling.

### 7. Autenticación con JWT y estado global

El login devuelve **access + refresh token**; `AuthContext` guarda el JWT en
`localStorage` y restaura el usuario al cargar. El `api-client` envía
`Authorization: Bearer` automáticamente; ante un **401** intenta un refresh
silencioso y reintenta la petición, y si falla cierra sesión. RBAC: el backend
valida permisos (401/403) y el front oculta rutas/acciones con `ProtectedRoute` y
`<Can>`.

### 8. Pruebas unitarias de lógica de negocio (xUnit)

xUnit en `backend/tests/Licoreria.UnitTests` con **116 tests** verdes, enfocados en
reglas de negocio: `MaquinaEstadosComandaTests` (transiciones), `ServicioCuentasTests`
(abonos con tasa y máquina de estados), `ServicioVentasTests` (pagos mixtos),
`CalculadoraCuentaTests`, `DetectorConflictosReservaTests`, `EvaluadorMermaTests`,
etc. Se ejecutan con
`dotnet test backend/tests/Licoreria.UnitTests/Licoreria.UnitTests.csproj` y corren
en CI sin infraestructura.

### 9. Aislamiento de capas y mocks con Moq

Los servicios de `Application` dependen de **interfaces** (`IUnitOfWork`,
`ICuentaRepository`, `IServicioFinanzas`…), nunca de EF Core directamente. Con
**Moq** se mockean esas dependencias (patrón AAA: Arrange-Act-Assert): por ejemplo,
`ServicioVentasTests` captura el callback de transacción con
`.Returns((op, ct) => op(ct))` y devuelve entidades simuladas, validando la
conversión Bs→USD sin tocar una base de datos real. El aislamiento garantiza que
solo se prueba la lógica del servicio.
