# Frontend · Licorería / Discoteca (Grupo 5)

Guía de contribución y **alcance pendiente de la Fase 4**. Léela completa antes de
tocar código. El backend (API v1 .NET 10 + PostgreSQL) ya está implementado; el
trabajo restante es construir el frontend React que lo consume **sin datos estáticos**.

> Documentación relacionada:
> - API para el front: `docs/dev/04-api/guia-frontend.md`
> - Endpoints: `docs/dev/04-api/endpoints.md`
> - Contrato: `openapi/licoreria.yaml`
> - Arquitectura front: `docs/dev/07-frontend/overview.md` y ADR `0003`.

---

## 1. Decisiones tomadas

- Monorepo de **dos apps** con workspaces npm: `public-web` (clientes) y `admin` (staff), según ADR 0003.
- **React 18 + Vite + TypeScript + Tailwind CSS**.
- **Context API** para estado global de sesión y tema:
  - `AuthContext.jsx` (JWT en `localStorage`).
  - `ThemeContext.jsx` (Azul UNET `#003366` ↔ Modo Oscuro, persistente).
  - `ModoContext.jsx` (Modo Licorería ↔ Modo Discoteca, persistente).
- Datos del servidor con **TanStack Query**; formularios con **React Hook Form + Zod**;
  gráficas con **Recharts**; tiempo real con **`@microsoft/signalr`**; toasts con **sonner**.
- RBAC: la API **siempre** valida en servidor; el front solo oculta rutas/acciones.

## 2. Estructura objetivo

```
frontend/
├── package.json                # workspaces + scripts
├── tsconfig.base.json
├── nginx.conf                  # fallback SPA (Docker)
├── packages/
│   ├── config/                 # eslint + tailwind preset (token UNET)
│   ├── types/                  # DTOs TS espejo de la API
│   ├── api-client/             # fetch + JWT + refresh + RFC7807 + paginación
│   └── ui/                     # design system (Button, Input, Table, Skeleton, Toast...)
└── apps/
    ├── admin/                  # SPA interna (foco Grupo 5)
    └── public-web/             # web pública (alcance secundario)
```

## 3. Puesta en marcha

```bash
# Requisitos: Node 20+, Docker (PostgreSQL), .NET 10 (solo backend)
cp ../.env.example ../.env

# 1) Backend + BD
docker compose up -d postgres
dotnet run --project ../backend/src/Licoreria.WebAPI   # http://localhost:5190

# 2) Frontend
npm install            # en frontend/
npm run dev -w apps/admin       # http://localhost:5173
npm run dev -w apps/public-web  # otro puerto
```

Variables de entorno de cada app (`.env.local`):

```
VITE_API_URL=http://localhost:5190
VITE_HUB_URL=http://localhost:5190/hubs/comandas
```

Credenciales de prueba: `admin@licoreria.com / admin123` (Admin) y
`cajero1@licoreria.com / cajero123` (Employee).

## 4. Contratos clave de la API

- **Auth (`/api/auth`)**: `POST /login` → `{ accessToken, refreshToken, rol, permisos[], expiraEn }`;
  `GET /me`; `POST /refresh`; `POST /logout`.
- **Recursos (`/api/v1/...`)**: enviar `Authorization: Bearer <accessToken>`.
- **Paginación**: `?page=1&pageSize=20` → `{ items, page, pageSize, totalItems, totalPages }` (máx. 100).
- **Errores RFC 7807** (`application/problem+json`): mostrar `title` + `detail` en un Toast.
  - `400` validación (`errors` por campo), `401` no autenticado, `403` sin permiso,
    `404`, `409` conflicto, `422` regla de negocio, `500`.
- **Enums** como texto (ej. `"Preparado"`, `"USD"`); fechas ISO 8601 UTC.
- **SignalR**: `/hubs/comandas?access_token=<jwt>`; `UnirseArea("barra"|"cocina"|"meseros")`.
  Eventos: `comanda:creada`, `comanda:actualizada`, `item:actualizado`, **`mesa:actualizada`**.

## 5. Contextos (interfaces esperadas)

```jsx
// AuthContext.jsx
{ usuario, rol, permisos, token,
  login(username, password), logout(), refrescar(),
  tienePermiso(clave), esAdmin }
```

- Guarda `accessToken`/`refreshToken` en `localStorage`; al montar, llama a `GET /api/auth/me`.
- El `api-client` reintenta una vez tras `401` con `POST /api/auth/refresh`.

```jsx
// ThemeContext.jsx
{ tema: 'unet' | 'oscuro', alternarTema(), setTema(t) }
// aplica clase/variables CSS y persiste en localStorage
```

```jsx
// ModoContext.jsx
{ modo: 'licoreria' | 'discoteca', setModo(m) }
```

## 6. RBAC (guards)

- `<ProtectedRoute>` exige sesión; redirige a `/login`.
- `<Can permiso="catalog:write">` oculta acciones.
- Rol `Employee`: **ocultar** rutas de administración (usuarios, roles, configuración,
  contenido, IA) y acciones de escritura. La API responde `403` igualmente.
- Permisos disponibles: `catalog:*`, `inventory:*`, `purchasing:*`, `sales:*`,
  `account:*`, `cash:*`, `club:*`, `reservation:manage`, `crm:*`, `finance:*`,
  `content:*`, `ai:*`, `security:*`.

## 7. Alcance pendiente (checklist para agentes)

Leyenda: `[ ]` pendiente · `[~]` en progreso · `[x]` hecho.

> **Estado (cierre Fase 4):** 7.1–7.5 y 7.8 completos. 7.6 cubre Catálogo, Usuarios,
> Clientes, Caja, Inventario, Compras y Contenido (restan Finanzas, IA y el editor de
> plano). 7.7 incluye el rediseño "Hungry Tiger" con vitrina 3D, menú, tasas y eventos
> (resta la reserva web con seña). El backend de la §8 ya está implementado.

### 7.1 Andamiaje
- [x] workspaces, `tsconfig.base`, ESLint/Prettier, Tailwind con token `#003366`
- [x] `packages/types` (DTOs), `packages/api-client`, `packages/ui`
- [x] `.env.local`, fuentes, layout responsive (Mobile First)

### 7.2 `admin` — Núcleo
- [x] `LoginPage` (`POST /api/auth/login`) + AuthContext + interceptor refresh
- [x] `AuthContext`, `ThemeContext`, `ModoContext` con persistencia
- [x] Layout (sidebar/header), selector de tema, selector de modo
- [x] Guards RBAC y ocultamiento de rutas admin
- [x] Toast global para RFC 7807

### 7.3 `admin` — Dashboard KPI  (`overflow-x-auto`)
- [x] Tarjetas KPI (`GET /api/v1/reportes/dashboard`)
- [x] **Mapa de calor de consumo por franja horaria** (`GET /api/v1/reportes/heatmap`)
- [x] **Diagnóstico de inventario** mín/máx con alertas (`GET /api/v1/reportes/inventario-salud`)
- [x] **Mermas vs ventas** (`GET /api/v1/reportes/mermas-vs-ventas`)
- [x] Skeletons y manejo de error

### 7.4 `admin` — Operación (Modo Discoteca)
- [x] **Plano interactivo SVG**: mesas/VIP desde `GET /api/v1/planos` + `/mesas`,
      color por estado (Libre/Reservada/Ocupada/En limpieza)
- [x] Abrir mesa (`POST /api/v1/cuentas`)
- [x] **Desalojo de mesa en tiempo real**: `POST /api/v1/mesas/{id}/desalojar` +
      evento `mesa:actualizada` → el resto de clientes ven la mesa **Libre**
- [x] Cuenta: comandas (`POST /cuentas/{id}/comandas`), máquina de estados
      Recibido→Preparado→Entregado (`PUT .../detalles/{detalleId}/estado`)
- [x] Abonos y **pagos mixtos USD/Bs** (`POST /cuentas/{id}/abonos`, `.../cerrar`),
      sincronizados con `GET /api/v1/tasas-cambio/actual`
- [x] Dividir cuenta (`POST /cuentas/{id}/dividir`)
- [x] **KDS Barra** en tiempo real (`/hubs/comandas`, área `barra`), tiempo transcurrido
      y botón "Marcar Preparado"
- [x] Reservas VIP: crear/gestionar, validar señas y pedidos anticipados
- [x] Mermas y cortesías (`GET/POST /api/v1/mermas`)

### 7.5 `admin` — POS (Modo Licorería)
- [x] Venta rápida de mostrador: búsqueda de productos, carrito, cobro mixto
      (`POST /api/v1/ventas`), comprobante, propina y descuento

### 7.6 `admin` — Catálogo / Inventario / Administración
- [x] Productos con variantes (crear/editar/eliminar), paginación server-side y búsqueda
- [x] Categorías y marcas (CRUD)
- [x] Usuarios: crear/editar, cambio de contraseña, revocar sesiones, baja lógica
- [x] Clientes (CRM): CRUD y acumular/canjear puntos
- [x] Caja: abrir, movimientos, arqueo y cierre (Z), historial
- [x] Inventario: existencias, kardex, ajustes, lotes y tomas físicas
- [x] Mermas y cortesías: registro e historial
- [x] Compras: proveedores, órdenes de compra, recepciones y cuentas por pagar
- [x] Contenido: páginas (secciones/bloques), eventos, horarios, local, menú y archivos
- [ ] Finanzas, IA y editor de plano (dnd-kit)

### 7.7 `public-web`
- [x] Menú digital, tasas del día y eventos (API pública, sin datos estáticos)
- [x] Rediseño "Hungry Tiger" (paleta dorado-sobre-óxido) y vitrina 3D del producto
- [ ] Reserva web con selección de mesa en plano y seña (`POST /api/v1/reservas`)

### 7.8 Calidad (definición de terminado)
- [x] Diseño **Mobile First** en todas las vistas
- [x] **Skeletons** en toda carga asíncrona
- [x] **Paginación en servidor** reutilizable
- [x] **Validación de formularios** en cliente (Zod) + errores del servidor por campo
- [x] Toasts amigables para RFC 7807
- [x] **Docker**: `Dockerfile` de cada app (node build → nginx) + servicio en `docker-compose.yml`

## 8. Backend añadido para la Fase 4 (ya implementado)

- [x] `GET /api/v1/reportes/heatmap` (consumo por día/hora)
- [x] `GET /api/v1/reportes/inventario-salud` (mín/máx + unidades de compra)
- [x] `GET /api/v1/reportes/mermas-vs-ventas`
- [x] `POST /api/v1/mesas/{id}/desalojar` + evento **`mesa:actualizada`**
- [x] `openapi/licoreria.yaml` y `docs/dev/04-api/endpoints.md` actualizados

## 9. Convenciones de contribución

- Ramas: `fase-4/<app>/<modulo>` (ej. `fase-4/admin/kds-barra`).
- Commits convencionales: `feat(admin): ...`, `fix(api-client): ...`, `docs(...)`.
- Un PR por módulo; describir endpoints usados y adjuntar captura.
- No commitear `.env`, `node_modules`, `dist`.
- Ejecutar `npm run lint` y `npm run build` antes del PR.

### Checklist de PR
- [ ] Consume API real (sin datos estáticos).
- [ ] Maneja `401/403/404/409/422` con Toast.
- [ ] Respeta RBAC (oculta lo no permitido).
- [ ] Responsive probado en móvil.
- [ ] Sin `console.log` ni `TODO` huérfanos.
