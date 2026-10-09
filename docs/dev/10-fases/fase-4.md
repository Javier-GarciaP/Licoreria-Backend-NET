# Fase 4 · Frontend React + Docker

> **Cómo usar este documento como diapositivas:** cada `##` es una lámina. La línea
> *«Dí: …»* es el mensaje clave a decir en voz alta; lo demás es material de apoyo.

---

## 1 · La fase en cifras

| Métrica | Valor |
| :--- | :--- |
| Apps React (workspaces) | **3** — `admin`, `servicio`, `public-web` |
| Paquetes compartidos | **5** — `types`, `api-client`, `ui`, `auth`, `config` |
| Pantallas en el panel interno | **40** páginas + 41 componentes |
| Pruebas backend | **145 verdes** (116 unitarias + 29 integración) |
| Pruebas frontend | **42 verdes** en 11 archivos (~13 s) |
| Servicios Docker | **6** + `pgadmin` opcional (perfil `tools`) |
| Código frontend | ~25 000 líneas TypeScript/TSX |
| Errores de build / tests fallidos | **0** |

> **Dí:** «Esta fase entrega tres aplicaciones React sobre la misma API, con tiempo real,
> seguridad de extremo a extremo y todo orquestado en Docker; 187 pruebas verdes y cero
> errores de compilación.»

---

## 2 · Objetivo y alcance

- **Objetivo:** implementar el frontend de la plataforma (panel interno, web de servicio
  y web pública) consumiendo la API v1 .NET + PostgreSQL, con orquestación Docker.
- **Entró en esta fase:** monorepo npm workspaces, design system propio, JWT + refresh en
  el cliente, RBAC en UI, SignalR, dashboard analítico, POS, plano interactivo, KDS,
  pagos mixtos USD/Bs, modo dual y Docker Compose completo.
- **No entró (siguiente iteración):** módulo de IA (generaciones y aprobación).

> **Dí:** «El alcance fue el frontend completo más la orquestación; lo que quedó fuera
> está declarado y documentado.»

---

## 3 · Stack tecnológico

| Capa | Tecnología |
| :--- | :--- |
| Build / bundler | **Vite** (dev con HMR, build con code-splitting) |
| UI | **React 18** + **TypeScript** + **Tailwind CSS** (tokens en `packages/config`) |
| Enrutamiento | **react-router-dom v6** (`createBrowserRouter`) |
| Estado de servidor | **TanStack Query** (`useQuery`/`useMutation` + invalidación) |
| Estado global | **Context API** (`AuthContext`, `ThemeContext`, `ModoContext`) |
| Formularios | **React Hook Form** + validación **Zod** (cliente) / FluentValidation (servidor) |
| HTTP | Wrapper propio de **fetch** en `packages/api-client` (JWT + refresh + RFC 7807) |
| Tiempo real | **SignalR** (`@microsoft/signalr`, hub `/hubs/comandas`) |
| Gráficas / UX | **Recharts**, **sonner** (toasts), **dnd-kit** (planos y KDS), skeletons |
| 3D (web pública) | **React Three Fiber** + glTF (vitrina de botella) |
| Pruebas FE | **Vitest** + **React Testing Library** + **MSW** (42) |
| Pruebas BE | **xUnit** + **Moq** (116) + integración con `WebApplicationFactory` (29) |
| Empaquetado | **npm workspaces** (monorepo) + **Docker Compose** (6 servicios) |

> **Dí:** «Cada capa tiene una tecnología elegida con propósito: nada de estado global
> improvisado ni de `fetch` disperso por los componentes.»

---

## 4 · Arquitectura del monorepo

ADR [0003](../adr/0003-frontend-react-tailwind.md).

```
frontend/                       backend/ (Onion, ya construida en fases 1-3)
├── apps/
│   ├── admin/        SPA interna (staff)        WebAPI → Application → Domain
│   ├── servicio/     mesonero / KDS                     ↑
│   └── public-web/   web pública                    Infrastructure
└── packages/
    ├── config/       tokens Tailwind + presets
    ├── types/        DTOs TypeScript espejo de la API
    ├── api-client/   fetch + JWT/refresh + RFC 7807
    ├── auth/         AuthContext, guards RBAC y login compartido
    └── ui/           design system (paleta lavanda; ver DESIGN.md)
```

Reglas: las apps **no** se importan entre sí; todo lo compartido vive en `packages/`;
las apps solo dependen de paquetes y de la API.

> **Dí:** «Un monorepo con workspaces: una sola instalación, un solo lint y dependencias
> compartidas sin duplicar código entre las tres apps.»

---

## 5 · Las tres aplicaciones

| App | Puerto | Para quién | Contenido clave |
| :--- | :---: | :--- | :--- |
| **admin** | `4173` | Staff / administración | Dashboard KPI, POS, cuentas, plano, inventario, compras, reportes, usuarios, CRM, caja, contenido |
| **servicio** | `4175` | Mesonero, barra y cocina | `MesoneroPage` (mesas, carta, comandas), `KdsPage` (tablero Kanban con arrastre), chat de staff |
| **public-web** | `4174` | Clientes | Menú digital, tasas, eventos, galería, **reserva web con seña** y vitrina 3D |

Módulos de administración: catálogo (productos con variantes), usuarios, clientes/CRM,
caja (apertura, arqueo Z), inventario (kardex, lotes, tomas, mermas), compras y cuentas
por pagar, contenido (páginas, eventos, menú con QR), tasas de cambio y salón
(editor de planos con `dnd-kit`).

> **Dí:** «Tres públicos distintos con tres experiencias distintas, sobre una única API.»

---

## 6 · Decisiones clave (y por qué)

- **Context API para estado global:** `AuthContext` (JWT en `localStorage`),
  `ThemeContext` (Claro/Oscuro persistente) y `ModoContext` (Licorería/Discoteca).
  Redux sería peso extra para tres flags compartidos.
- **TanStack Query para el servidor:** caché, reintentos e invalidación; elimina
  `useEffect` de carga y peticiones duplicadas.
- **Tema (desviación aprobada):** se conserva la paleta **lavanda/álamo** del design
  system (`frontend/DESIGN.md`); el «Azul UNET» `#003366` **no se adopta** porque
  `DESIGN.md` §7 lo prohíbe como acento y el cambio rompería la deuda visual ya cerrada.
- **RBAC en dos capas:** la API valida (401/403) y el front oculta rutas/acciones con
  `ProtectedRoute` y `<Can>` — la UI nunca es la única barrera.
- **Accesibilidad básica:** modales con `role="dialog"`/`aria-modal`, cierre con
  `Escape`, foco gestionado, `scope="col"` en tablas y enlace «saltar al contenido».

> **Dí:** «Ninguna decisión fue por moda: cada una resuelve un problema concreto y la
> desviación de tema está registrada y justificada, no escondida.»

---

## 7 · Tiempo real: del mesonero a la barra

```
Mesonero (app servicio)        API .NET (.NET + SignalR)        Barra (KDS)
──────────────────────         ─────────────────────────        ────────────
1. Enviar pedido  ──POST──▶    guarda comanda + verifica stock
                               emite "comanda:creada"   ──▶    🔔 sonido + refresca
                                                               el tablero Kanban
2.                                                      barra arrastra el ítem a
                                                          "En proceso"
                               valida máquina de estados
                               emite "item:actualizado" ──▶    se repinta
3.  ◀── toast «En preparación»  (grupo meseros)
4.  ◀── toast «Pedido listo» cuando la barra marca Preparado
```

- **Hub:** `/hubs/comandas` con `[Authorize]` (JWT) y grupos `barra`, `cocina`,
  `meseros`, `mesas`, `staff`; los clientes se unen con `UnirseArea(area)`
  (`ComandasHub.cs`) y se re-únen tras cada reconexión.
- **El evento solo lleva IDs** (`cuentaId`, `comandaId`, `detalleId`, `estado`); cada
  cliente relee los datos con TanStack Query (`invalidateQueries`). Así nadie muestra
  información vieja ni hay que sincronizar payloads.
- **Desalojo de mesa:** `POST /api/v1/mesas/{id}/desalojar` emite `mesa:actualizada`
  y el plano se refresca para todo el personal.
- **En Docker:** el `edge` (nginx) hace proxy con *upgrade* de WebSocket en `/hubs/`.

> **Dí:** «El patrón es radio por grupos + revalidación: el mensaje solo avisa *qué cambió*
> y cada pantalla vuelve a leer la fuente de verdad.»

---

## 8 · Seguridad de extremo a extremo

| Capa | Mecanismo |
| :--- | :--- |
| Login | `POST /api/v1/auth/login` → **access + refresh token** |
| Cliente | `AuthContext` persiste en `localStorage`; `api-client` adjunta `Authorization: Bearer` |
| Renovación | ante **401**, *refresh* silencioso y reintento de la petición; si falla, logout |
| RBAC | backend `[Authorize(Roles=…)]` → 401/403; frontend `ProtectedRoute` + `<Can>` (oculta rutas y acciones) |
| Errores | parser **RFC 7807** en `api-client` → toasts `sonner` con mensaje del servidor |

> **Dí:** «La UI oculta lo que el usuario no puede hacer, pero quien igual lo pida a la
> API recibe 401/403: la autoridad siempre es el servidor.»

---

## 9 · Casos de negocio destacados

- **Modo dual Licorería / Discoteca:** `ModoProvider` en `main.tsx` + conmutador en el
  header; los helpers `filtrarGrupos`/`puedeAcceder` filtran **antes** del bypass de
  admin (Licorería oculta Salón/`/plano`/`/cuentas`; Discoteca = navegación completa).
- **Máquina de estados de comandas:** `MaquinaEstadosComanda` inyectada en
  `ServicioCuentas.CambiarEstadoItemAsync` — `Recibido → EnProceso → Preparado →
  Entregado` (con `Cancelado` terminal); prohíbe regresiones, ignora no-ops y deriva el
  estado de la comanda. El KDS trabaja en 3 columnas (Recibido / En proceso / Terminado).
- **Pagos mixtos USD/Bs:** el backend convierte Bs → USD con la tasa Paralelo vigente
  (validando `tasa > 0`) y persiste monto/moneda originales; el front tiene selector de
  moneda en cobro, abonos, `PosCarta` y POS, con `formatUSD`/`formatBS`.
- **Inventario en servicio:** al servir un ítem (`Preparado`/`Entregado`) se descuenta
  por kardex y al cancelar se reintegra; con stock insuficiente la API responde **422**.
- **Dashboard sin desborde:** heatmap dentro de `overflow-x-auto` con `min-w-[560px]` y
  bento de KPIs con scroll horizontal en móvil.

> **Dí:** «Los tres casos complejos —modo dual, estados de comanda y pagos mixtos—
> están conectados de punta a punta, no son maquetas.»

---

## 10 · Requerimientos del Grupo 5 cubiertos

| Requerimiento | Implementación |
| :--- | :--- |
| Modo dual Licorería / Discoteca | `ModoProvider` + `filtrarGrupos`/`puedeAcceder` por ítem/grupo |
| Plano interactivo | `PlanoPage` (SVG) con estados Libre/Reservada/Ocupada/En limpieza |
| KDS Barra | `KdsPage` con SignalR (área `barra`) y arrastre entre 3 columnas |
| Máquina de estados de comandas | `MaquinaEstadosComanda` en `ServicioCuentas.CambiarEstadoItemAsync`; `Entregado` se marca desde `PosCarta` |
| Conflictos de reservas VIP | `ReservasPage` con creación, señas y pedidos |
| Mermas y cortesías | `MermasPage` + `EvaluadorMerma` integrado en `RegistrarMermaAsync` |
| Pagos mixtos USD/Bs | Selector en `CuentaPage`/`PosCarta`/`PosPage`; conversión con tasa vigente en el backend |
| Dashboard analítico | `DashboardPage` con heatmap, salud de inventario y mermas vs ventas |
| RBAC Guards | `ProtectedRoute` y `Can` |
| RFC 7807 | parser en `packages/api-client` + toasts |
| Mobile First / Skeletons / paginación servidor / validación / Docker | aplicados en toda la app |

---

## 11 · Backend añadido para esta fase

- `GET /api/v1/reportes/heatmap` — consumo por día de la semana y hora.
- `GET /api/v1/reportes/inventario-salud` — clasificación con mínimos/máximos.
- `GET /api/v1/reportes/mermas-vs-ventas` — comparativo valorizado.
- `POST /api/v1/mesas/{id}/desalojar` + evento SignalR `mesa:actualizada`.
- `MesaDto` enriquecido con `cuentaId` y `reservada`.
- Guard de turno abierto (422 sin sesión de caja), `POST /cuentas/{id}/reabrir` y
  auto-desalojo al cerrar el turno (evento `mesa:actualizada`).

---

## 12 · Orquestación con Docker

```
                 navegador
        ┌────────────┼────────────┐
     :4173        :4174        :4175
   (admin)    (public-web)  (servicio)     ← nginx sirve cada SPA
        └────────────┼────────────┘
                   :8090
                 (edge)  ── proxy nginx: /api, /swagger, /hubs (WebSocket), /uploads
                     │
                   :5190
                  (api)  ── .NET 10: aplica migraciones + seed al arrancar
                     │
                  :5432
               (postgres) ── PostgreSQL 15 (volumen con datos persistentes)
```

| Servicio | Imagen / build | URL local |
| :--- | :--- | :--- |
| `postgres` | `postgres:15-alpine` | `localhost:5432` |
| `api` | build `backend/src/Licoreria.WebAPI/Dockerfile` | `http://localhost:5190` · Swagger `…/swagger` |
| `admin` | build `apps/admin/Dockerfile` (node → nginx) | `http://localhost:4173` |
| `public-web` | build `apps/public-web/Dockerfile` | `http://localhost:4174` |
| `servicio` | build `apps/servicio/Dockerfile` | `http://localhost:4175` |
| `edge` | `nginx:alpine` + `infra/nginx/nginx.conf` | `http://localhost:8090` |

- Todo parametrizable por `.env` (puertos, credenciales, `ASPNETCORE_ENVIRONMENT`),
  con plantilla versionada en `.env.example`.
- Swagger UI habilitado en el despliegue Docker vía `Swagger:Enabled=true`.
- Un solo comando: `docker compose up -d --build`.

> **Dí:** «Seis servicios y un proxy único de entrada: el navegador solo conoce el edge,
> y el WebSocket de SignalR cruza el nginx con upgrade correcto.»

---

## 13 · Calidad: pruebas y CI

| Suite | Comando | Resultado |
| :--- | :--- | :--- |
| Unitarias backend | `dotnet test` (`Licoreria.UnitTests`) | **116/116** |
| Integración backend | `dotnet test` (`Licoreria.IntegrationTests`, `WebApplicationFactory`) | **29/29** |
| Frontend | `npm test` (Vitest + RTL + MSW) | **42/42** (~13 s) |
| Tipos / lint | `npm run typecheck`, `npm run lint` | 0 errores |

Casos representativos: `Comanda_ConStockInsuficiente_Devuelve422`,
`Item_AlServirse_DescuentaStockYAlCerrarNoDuplica`, `MaquinaEstadosComandaTests`,
`ServicioVentasTests` (pagos mixtos) y `navigation.test.ts` (filtrado por modo).

**CI (GitHub Actions):** `backend.yml` (build + tests con servicio PostgreSQL),
`frontend.yml` (lint → typecheck → tests → build), `docs.yml` (sitio de documentación).

**Batería de cierre de fase (ejecutada en cada entrega):**
`dotnet build` → `dotnet test` → `npm run typecheck` → `npm run lint` → `npm test` →
`npm run build`.

> **Dí:** «Ninguna entregas se cerró sin las seis verificaciones en verde; la CI lo
> garantiza también en cada push.»

---

## 14 · Cómo se implementó: 5 brechas cerradas

Sobre una base ya funcional (commit `917d604`, 94 tests backend y 33 frontend verdes):

1. **Modo dual real:** el `ModoContext` existía pero no filtraba nada. Se añadió el campo
   `modos` por ítem/grupo en `navigation.tsx` y los helpers `visiblePorModo`,
   `filtrarGrupos` y `puedeAcceder` (el chequeo de modo ocurre **antes** del bypass de
   admin). `ModoProvider` se montó en `main.tsx` (envolviendo `ErrorBoundary`, `Toaster`
   y `AuthProvider`), el conmutador en el header de `AppShell` y `App.tsx` aterriza en
   `/pos` (licorería) o `/` (discoteca). +9 tests en `navigation.test.ts`.
2. **Dashboard sin desborde:** heatmap en `overflow-x-auto` con `min-w-[560px]` interno
   y bento de KPIs con scroll horizontal en móvil.
3. **Máquina de estados conectada:** reescrita sobre el enum `EstadoItemComanda` e
   inyectada en `ServicioCuentas.CambiarEstadoItemAsync` (valida transiciones con
   `ReglaNegocioException`, ignora no-ops, deriva el estado de la comanda +
   notificación SignalR). `Entregado` se marca desde `PosCarta`, manteniendo el KDS en
   3 columnas.
4. **Pagos mixtos USD/Bs:** conversión Bs → USD con tasa Paralelo vigente en abonos y
   ventas (validando `tasa > 0`), persistiendo monto/moneda originales; selector de
   moneda y formato en el front.
5. **`EvaluadorMerma` integrado:** `ServicioInventario.RegistrarMermaAsync` consume los
   movimientos sugeridos (Merma + Cortesía opcional) con paridad de comportamiento; el
   evaluador pasó a aceptar `decimal`.

Verificación de cierre siempre igual: build 0 errores, 116 tests, typecheck, lint y
42 tests front; los cambios quedaron en este checklist, `frontend/README.md` y el
`CHANGELOG`, con la **desviación de tema aprobada** registrada.

---

## 15 · Demo en vivo (≈5 minutos)

1. `docker compose up -d --build` → levanta los 6 servicios (`docker compose ps`).
2. **Panel interno** `:4173` → login `admin@licoreria.com` / `demo123` → Dashboard
   (heatmap, KPIs) → POS: buscar producto, cobrar en USD y Bs.
3. **App de servicio** `:4175` → login `mesero1@licoreria.com` / `demo123` → aterriza en
   `/mesonero`: abrir mesa y enviar una comanda a **Barra**.
4. Cerrar sesión y entrar con `barra1@licoreria.com` (misma contraseña) → aterriza en
   `/kds`: el pedido llega con sonido → arrastrarlo a *En proceso* y *Terminado*.
5. Volver al mesonero: los toasts «En preparación» y «Pedido listo» aparecen solos;
   cerrar con el conmutador **Licorería/Discoteca**; Swagger en `:5190/swagger`.

---

## 16 · Limitaciones conocidas y próximos pasos

| Limitación | Impacto | Plan |
| :--- | :--- | :--- |
| `UnirseArea(area)` no valida el rol | cualquier JWT autenticado podría escuchar otra área | validar rol/área en el hub con los claims del JWT |
| Varias conexiones SignalR por pestaña | una conexión por componente que escucha | conexión única compartida en `packages/` con rejoin |
| `DbUpdateConcurrencyException` responde 500 | carrera entre dos escrituras no se traduce a 409 | mapear a `ConflictoException` en `ExceptionMiddleware` |
| Sin `CHECK (cantidad >= 0)` en la BD | la guarda de stock negativa es solo de aplicación | restricción CHECK + `SELECT … FOR UPDATE` en kardex |
| Módulo de IA pendiente | fuera de esta fase | generación y aprobación en iteración siguiente |

> **Dí:** «Conozco los huecos, están priorizados con mitigación, y ninguno afecta el
> flujo que acaban de ver.»

---

## 17 · Cuestionario de defensa (Fase 4)

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

### 10. ¿Cómo llega en tiempo real lo que pide el mesonero a la barra?

SignalR con el hub `/hubs/comandas` y **grupos por área** (`barra`, `cocina`,
`meseros`). El backend emite `comanda:creada`/`item:actualizado` **después** de guardar
en BD, enviándolo al grupo del área y al de los meseros. El KDS de la barra está suscrito
a su grupo: al recibir el evento reproduce un sonido y hace `invalidateQueries(['kds'])`
para releer y repintar el tablero. El evento solo lleva IDs, así que siempre se muestra
la versión vigente de la base de datos.

### 11. ¿Qué pasa si se cae la conexión con el hub?

El cliente usa `withAutomaticReconnect()` y, al reconectar, vuelve a invocar
`UnirseArea(area)` para recuperar su grupo. Los eventos emitidos durante la desconexión
se pierden, por eso la UI también revalida al recuperar el foco/la consulta; es una
limitación documentada con plan de mejora (ver lámina 16).

### 12. ¿Cómo se evita el stock negativo en ventas simultáneas?

Tres defensas: `StockProducto.AplicarMovimiento` rechaza saldos negativos (dominio),
`ServicioKardex.VerificarStockAsync` valida antes de descontar (→ **422**) y la
concurrencia optimista con `xmin` de PostgreSQL hace que, si dos transiciones compiten
por la misma fila, la segunda falle en el `UPDATE` y se revierta en lugar de dejar el
stock en negativo.

### 13. ¿Por qué Docker y no desplegar a mano?

Repetibilidad: `docker compose up -d --build` levanta las mismas seis piezas en cualquier
máquina (migraciones y seed automáticos al arrancar la API), con puertos y credenciales
en `.env`, y el `edge` unifica el origen de la API, el WebSocket y las SPAs.

### 14. ¿Cómo sabes que nada se rompió al cerrar la fase?

Batería fija: `dotnet build` (0 errores), `dotnet test` (145), `npm run typecheck`,
`npm run lint` y `npm test` (42); la misma batería corre en GitHub Actions en cada push
(`backend.yml`, `frontend.yml`).

---

## Rúbrica cumplida

| Criterio | Evidencia |
| :--- | :--- |
| SPA con enrutamiento cliente | react-router-dom v6 + `ProtectedRoute` (40 páginas en admin) |
| Estado global y de servidor | Context API (`Auth`/`Theme`/`Modo`) + TanStack Query |
| Cliente API con JWT y errores | `packages/api-client`: Bearer, refresh ante 401, RFC 7807 → toasts |
| RBAC en UI | `ProtectedRoute` y `<Can>`; el backend sigue siendo la autoridad (401/403) |
| Tiempo real | SignalR `/hubs/comandas` con grupos de área (KDS, mesas, chat) |
| Design system y accesibilidad | `packages/ui` + `DESIGN.md`; diálogos ARIA, foco y «saltar al contenido» |
| Pruebas | 42 frontend (Vitest/RTL/MSW) + 145 backend, todo en CI |
| Orquestación | Docker Compose de 6 servicios con edge nginx (WebSocket) |
| Documentación | Este checklist, `frontend/README.md`, `DESIGN.md` y `CHANGELOG` al día |

## Documentación relacionada

- [Guía de contribución del frontend](../../../frontend/README.md)
- [Design system (DESIGN.md)](../../../frontend/DESIGN.md)
- [ADR 0003 · Frontend React + Tailwind](../adr/0003-frontend-react-tailwind.md)
- [Arquitectura Onion](../02-arquitectura/onion.md)
- [Errores RFC 7807](../04-api/errores-rfc7807.md)
- [Evidencias por fase](../../assets/evidencias/README.md)
- [CHANGELOG](../../../CHANGELOG.md)
