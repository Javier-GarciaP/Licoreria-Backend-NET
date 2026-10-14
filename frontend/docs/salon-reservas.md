# Salón y Reservas · Mesas desde el editor y detalle de reserva

> Documento de trabajo. Registra **qué se hizo** y **con qué reglas**, para que
> cualquier pantalla nueva mantenga el mismo patrón. Complementa a
> `rediseno-convencional.md` (migración a shadcn/ui) y a `DESIGN.md`.

---

## 1. Salón · Mesas desde el editor

### Qué se hizo

- Se retiró la **gestión de zonas** de la UI del salón (card de Zonas, CRUD y
  formulario con posición/color). Las zonas siguen viviendo en el backend, se
  dibujan como regiones en el mapa y filtran el feed en vivo; solo se quitó la
  pantalla de administración.
- El apartado "Zonas y mesas" pasa a ser **"Mesas"** (`/salon/mesas`):
  - `ZonasMesasPage.tsx` → **`SalonMesasPage.tsx`**.
  - Tabla con **información básica**: Número · Capacidad · Forma · En plano ·
    Estado · Acciones (Editar/Eliminar).
  - **Sin botón "Nueva mesa"**: las mesas se crean desde el editor de planos.
  - Edición por **formulario** con Número, Capacidad, Forma y Activa. La zona y
    la geometría se conservan ocultas (se envían los valores actuales).
  - Al cambiar la **forma**, se sincronizan los elementos enlazados de los
    planos para que el mapa refleje el cambio.
- El **editor de planos crea mesas**:
  - Al colocar una figura `mesa_redonda` / `mesa_cuadrada` / `mesa_rectangular`
    se abre un modal con **Número + Capacidad** → `POST /mesas` → se enlaza
    `mesaId` y el número se usa como etiqueta.
  - El inspector muestra un botón **"Crear mesa"** cuando el elemento es mesa y
    no está enlazado; se conserva el selector **"Mesa operativa"**.
  - Al **guardar el plano** se sincroniza `forma/posX/posY/ancho/alto` de cada
    mesa enlazada (`PUT /mesas/:id`) para que el listado "atraiga" la geometría
    del editor.
- Nuevo helper **`apps/admin/src/lib/salon.ts`**: `FORMAS_MESA`, `etiquetaForma`,
  `formaElementoDeMesa` / `formaMesaDeElemento` (mapeo `redonda` ↔
  `mesa_redonda`, etc.), `zonaPorDefecto()` (crea la zona `"General"` con
  ancho/alto 0 si no existe, para cubrir el `ZonaId NOT NULL` del backend) y
  `planosDeMesa()`.

### Archivos

| Ruta | Cambio |
| :--- | :--- |
| `apps/admin/src/pages/SalonMesasPage.tsx` | Renombrado desde `ZonasMesasPage`; listado básico + edición |
| `apps/admin/src/lib/salon.ts` | Nuevo; helpers del salón |
| `apps/admin/src/pages/SalonLayout.tsx` | Pestaña "Mesas" → `/salon/mesas` |
| `apps/admin/src/App.tsx` | Ruta `/salon/mesas` |
| `apps/admin/src/pages/EditorMapaPage.tsx` | Modal "Nueva mesa" + sincronización de geometría al guardar |
| `apps/admin/src/components/mapa/EditorOverlay.tsx` | Botón "Crear mesa" en el inspector |

---

## 2. Reservas · Detalle de reserva

### Qué se hizo

- Nueva página **`/reservas/:id`** (`ReservaPage.tsx`), siguiendo el patrón de
  detalle de `CuentaPage`.
- La agenda (`ReservasPage.tsx`) queda como listado: el menú de acciones ahora
  navega a **"Ver detalle y pedidos"**. Se eliminaron del listado las modales de
  pedidos y señas (y su lógica).
- La página de detalle organiza el contenido en **dos columnas + señas abajo**:
  - **Izquierda** (ancha `1.4fr`): **Pedidos anticipados** — la lista del pedido
    general, que crece, con SKU, subtotal, total y botón "Quitar".
  - **Derecha** (`1fr`): **Agregar pedido** — picker visual con **alta directa**
    (ver §3).
  - **Abajo a lo ancho**: **Señas** — lista de pagos con Validar/Rechazar.
- Cabecera con **botón volver** (patrón unificado, ver §4) y acciones de estado
  (Confirmar / Cancelar según `Pendiente` / `Confirmada`).

---

## 3. Picker visual de productos (alta directa)

Componente **`apps/admin/src/components/reservas/PanelAgregarPedido.tsx`**,
adaptado del `AgregarComandaModal` del módulo de cuentas:

- **Buscador** + **chips de categorías** + **cuadrícula de productos** con
  imagen (`urlDeImagen`), nombre y rango de precio.
- Producto con **una variante** → un clic lo agrega **directo** al pedido
  (`POST /reservas/:id/pedidos` con cantidad 1 y refresco de la lista).
  Con **varias variantes** → modal `sm` para elegir presentación y luego alta.
- **Sin acumulador ni botón "Enviar"**: cada clic es una alta inmediata al
  pedido general, porque los pedidos pueden crecer y una modal no da buen flujo.

---

## 4. Patrón de detalle unificado (sin migas + botón volver)

Se replicó el patrón que ya usaban el editor de planos y el formulario de
producto (ocultar breadcrumbs y mostrar un botón `ArrowLeft` de regreso):

- `apps/admin/src/components/AppShell.tsx`: se ocultan los breadcrumbs en las
  rutas de detalle `/reservas/:id` y `/cuentas/:id`, además de las ya cubiertas
  `/productos/nuevo`, `/productos/:id/editar`, `/salon/planos/` y `/plano`.
- `ReservaPage.tsx` y `CuentaPage.tsx`: cabecera con `Button ghost` + `ArrowLeft`
  (→ `/reservas` y `/cuentas`, respectivamente), título + subtítulo a la
  izquierda y acciones a la derecha (`ml-auto`), con `border-b` bajo la cabecera.
- Antes `CuentaPage` usaba `PageHeader` con breadcrumbs; ahora ambas páginas de
  detalle comparten el mismo encabezado de regreso.

---

## 5. Reglas a mantener

1. **Detalle de una entidad** → ruta `/:recurso/:id` con cabecera "volver + título + acciones".
2. **Sin breadcrumbs en detalle/editor** → se ocultan en `AppShell`; mantener el regex actualizado al añadir rutas.
3. **Listas que crecen** (pedidos, consumos) → en página, nunca en modal.
4. **Añadir líneas** → picker visual de productos (imagen + rango de precio) con alta directa; modal solo para elegir variante.
5. **Geometría de la mesa** → el editor es la fuente de verdad; el listado solo la refleja (sincronización al guardar el plano).
6. **Zonas** → se conservan en backend / mapa / feed; la UI del salón no las administra. Las mesas nuevas usan la zona por defecto `"General"`.

---

## 6. Verificación

```bash
npm run lint
npm run typecheck -w @licoreria/admin
npm test
```

> Probar el flujo: editor de planos (colocar mesa → crear con número/capacidad →
> guardar → verla en `/salon/mesas`), agenda → detalle de reserva (agregar
> pedido directo, quitar, validar seña, confirmar/cancelar) y cuenta
> (`/cuentas/:id`) con su botón volver.