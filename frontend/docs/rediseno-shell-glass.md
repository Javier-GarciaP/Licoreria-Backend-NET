# Rediseño · Shell de vidrio (layout flotante)

> Documento de trabajo. Registra **qué se hizo**, **con qué reglas** y **qué sigue**,
> para que cualquier pantalla nueva mantenga el mismo patrón.
>
> Alcance de esta iteración: **solo el maquetado** — sidebar, header y canvas
> principal. No se toca el interior de los módulos todavía.

---

## 1. Objetivo

Sustituir el layout rígido de bordes anclados a la pantalla por un **shell de
paneles flotantes de vidrio**: cada zona (sidebar, header, canvas) es una
superficie redondeada, translúcida y separada del borde de la pantalla y de las
demás por **el mismo margen**. La estructura de los paneles sigue el patrón de
Supabase (bordes hairline, sin sombras duras, tipografía 500, vocabulario de
radios) con el añadido de un acabado *glass* tipo iOS.

### Restricciones que se respetaron

- **La paleta no cambia.** Se reutilizan los tokens existentes (`surface`, `ink`,
  `accent` violeta `#5433eb`, `violet`). El vidrio se consigue con opacidad y
  desenfoque, no con colores nuevos.
- **Sin sombras para elevar.** La separación la hace el borde hairline + el salto
  de superficie, según el patrón de Supabase.
- **Un solo gesto llamativo.** El único "efecto" es el vidrio y su luz ambiental;
  el resto queda en silencio.

---

## 2. Reglas del patrón (a mantener en adelante)

### Superficies de vidrio

Dos utilidades definidas en `packages/ui/src/styles/theme.css`:

| Clase | Uso | Fondo | Desenfoque |
| :-- | :-- | :-- | :-- |
| `.glass-panel` | Zonas grandes: sidebar y canvas | `surface / 0.55` | `blur(20px) saturate(150%)` |
| `.glass-bar` | Barras delgadas: header y bottom-nav | `surface / 0.72` | `blur(16px) saturate(150%)` |

Ambas comparten **borde hairline** `ink / 0.06`. El fondo de cada clase no debe
sobreescribirse con utilidades `bg-*`; el borde tampoco con `border-*`.

Fallbacks incluidos en la propia hoja:

- `@supports not (backdrop-filter…)` → sube la opacidad (94 % / 97 %) para no
  perder legibilidad.
- `@media (prefers-reduced-transparency: reduce)` → fondo casi opaco y sin
  desenfoque (respeto por la preferencia de iOS/macOS).

### Geometría del shell — una sola cota

**Regla dura:** el margen contra el borde de la pantalla, el gap entre paneles y
el padding interior de cada panel son **el mismo valor**:

| Token | Móvil | Escritorio |
| :-- | :-- | :-- |
| Gutter del shell | `16px` (`p-4` / `gap-4` / `inset-4`) | `24px` (`lg:p-6` / `lg:gap-6`) |

Se aplica de forma idéntica en:

- **Contenedor exterior**: `h-dvh`, `p-4 lg:p-6`, fondo `bg-canvas`. El shell no
  hace scroll; scrollea el canvas por dentro.
- **Retícula interior**: `flex` en fila, `gap-4 lg:gap-6`, `max-w-[1800px]`
  centrado. El mismo gap separa sidebar↔contenido y header↔canvas.
- **Padding lateral de los paneles**: `aside` `px-4 lg:px-6`; `header`
  `px-4 lg:px-6`; `main` `p-4 lg:p-6`. Así el contenido queda a la misma
  distancia del panel que los paneles entre sí.
- **Radios del shell**: `rounded-3xl` (24 px). Se eligió por encima de los 16 px
  de tarjeta de Supabase para que el panel grande tenga la curva amplia del
  vidrio móvil y armonice con las tarjetas internas (hoy 28 px).
- **Luz ambiental**: dos halos de `bg-accent/10` y `bg-violet/10` con
  `blur(130px)` detrás de los paneles. Sin ellos el vidrio no tiene nada que
  desenfocar. Es decoración funcional, no color nuevo.

> Para cambiar la cota del shell se tocan **los tres sitios a la vez**. Nunca
> dejar el margen exterior distinto del gap o del padding interno.

---

## 3. Qué se hizo (tareas)

### T1 · Utilidades de vidrio
- **Archivo:** `packages/ui/src/styles/theme.css`
- Se añadieron `.glass-panel` y `.glass-bar` con sus dos fallbacks
  (`@supports`, `prefers-reduced-transparency`).
- Son aditivas: `public-web` importa el mismo `theme.css` pero no las usa, así que
  no le afectan.

### T2 · Lienzo más ancho
- **Archivo:** `packages/config/tailwind.preset.cjs`
- `maxWidth.page`: `1200px` → `1600px`. El canvas ahora es amplio; las páginas que
  usan `mx-auto max-w-page` aprovechan el ancho sin quedar encajonadas en el
  centro.

### T3 · Reestructura del `AppShell`
- **Archivo:** `apps/admin/src/components/AppShell.tsx`
- Antes: sidebar `sticky h-dvh border-r`, header `sticky border-b`, `main` a
  sangre. Ahora:
  - Contenedor raíz `h-dvh overflow-hidden p-4 lg:p-6`.
  - `aside` → `glass-panel rounded-3xl` con `overflow-hidden`; su `nav` interna
    hace scroll (`app-scroll`, `min-h-0`).
  - Columna central `flex flex-col gap-4 lg:gap-6` con `header` (`glass-bar
    rounded-3xl`) y `main` (`glass-panel rounded-3xl`, `overflow-y-auto min-h-0`).
  - `main` conserva `id="contenido"` para el skip-link.
- Se mantuvo intacta toda la lógica: RBAC (`visibles`), modo, tema, sesión.

### T4 · Luz ambiental
- Incluida en T3: capa `aria-hidden` `absolute inset-0 z-0` con dos halos
  (`bg-accent/10`, `bg-violet/10`, `blur-[130px]`). Los paneles van en `z-10`.

### T5 · Bottom-nav móvil flotante + accesibilidad
- La navegación móvil pasó de `inset-x-0 bottom-0 border-t` a
  `glass-bar fixed inset-x-4 bottom-4 rounded-3xl`: mismo margen que el resto.
- `main` lleva `pb-28` en móvil para que el contenido no quede bajo la barra.
- Se conservan `aria-label`, skip-link y orden de foco.

### T6 · Igualar la cota externa con la interna
- **Archivo:** `apps/admin/src/components/AppShell.tsx`
- El margen exterior y los gaps pasaron de `p-3/gap-3 lg:p-4/gap-4` a
  **`p-4/gap-4 lg:p-6/gap-6`**, y el `main` de `lg:p-8` a `lg:p-6`, para que el
  espacio de los lados coincida exactamente con el espacio interno. Ver la tabla
  de "una sola cota" arriba.

### T7 · Header solo móvil
- **Archivo:** `apps/admin/src/components/AppShell.tsx`
- El `<header>` recibió `lg:hidden`. En escritorio el header era 100 % duplicado:
  marca, cambio de modo, cambio de tema y sesión ya viven en el sidebar; el único
  botón exclusivo (logout) estaba oculto con `lg:hidden`.
- Con el header en `display:none`, `main` pasa a ser el único hijo visible de la
  columna y ocupa toda su altura (el `gap` no se aplica a hijos ocultos) → el
  canvas gana ~56–64 px y su borde superior queda alineado con el del sidebar.
- Se retiraron los `lg:hidden` internos (marca, logout) que quedaron redundantes.
- En móvil no cambia nada: el header sigue aportando marca + modo/tema/logout,
  porque el sidebar no existe.

---

## 4. Cómo mantener el patrón

1. **Una zona nueva = `glass-panel` o `glass-bar`.** Nunca definir fondo/borde a
   mano para una superficie del shell.
2. **Respetar la cota única.** Todo panel flotante usa el mismo `p-4/gap-4
   lg:p-6/gap-6`. Nada pegado al borde de la pantalla y nada con un margen
   distinto al interno.
3. **Nada de sombras para elevar.** Para separar, borde hairline o subir de
   superficie. Las sombras existentes (`shadow-card`, `shadow-glow`) se reservan
   para elementos que de verdad flotan sobre el contenido (modales, menús).
4. **La luz ambiental no se duplica.** El glow vive una sola vez en `AppShell`.
5. **No introducir color fuera de la paleta.** El vidrio se logra con alfa +
   `backdrop-filter`.
6. **El header del shell es solo móvil.** En escritorio los controles globales
   (marca, modo, tema, sesión) viven en el sidebar; el header lleva `lg:hidden`.
   No reinstaurar un header de escritorio sin una razón funcional nueva (p. ej.
   la barra de comando de la opción B).

---

## 5. Pendiente (siguientes tareas pequeñas)

- [x] **Alinear el vocabulario de radios**: tarjetas 16 px, inputs 8 px, botones
      píldora (patrón Supabase). → **Hecha en §7 (T8).**
- [x] **Mover las sombras de tarjeta a borde**: `shadow-card` retirado de `Card`,
      `EmptyState` y `Skeleton`; se conserva en `Modal` (sí flota). → **Hecha en
      §7 (T8).**
- [~] **Tipografía**: `tabular-nums` ya aplicado en el dashboard; **pendiente** la
      cara mono tabular dedicada y bajar el tope de peso a 500 de forma global.
- [ ] **Agrupar la navegación por dominio** en el sidebar (hoy 18 ítems planos).
- [x] **Extender el vidrio** a tarjetas, modales y toasts (`.glass-card` global).
      → **Hecha en §8 (V1/V2).**
- [ ] Revisar `prefers-reduced-motion` en el botón de tema y en scrolls.

---

## 6. Verificación de esta iteración

```bash
npm run typecheck -w apps/admin
npm run build -w apps/admin
```

> Nota: `backdrop-filter` requiere un navegador moderno; los fallbacks ya cubren
> el resto. Probar visualmente en tema Oscuro y Azul UNET.

---

## 7. T8 — Vocabulario de radios y elevación

> **Estado: EJECUTADA.** Ver "Resultado" al final de la sección.

### Objetivo

Alinear la geometría de los componentes internos con el patrón de Supabase para
que el vidrio del shell y las tarjetas compartan un mismo vocabulario de radios,
y sustituir la elevación por sombra por elevación por borde.

### Cambios concretos

1. **Tokens de radio** — `packages/config/tailwind.preset.cjs`
   - `borderRadius.card`: `28px` → `16px` (tarjetas).
   - `borderRadius.inner`: `20px` → `12px` (medios/tablas embebidas).
   - `borderRadius.pill`: sin cambios (`9999px`).
   - Añadir `borderRadius.control: '8px'` (inputs/selects).
2. **Componentes afectados** — `packages/ui/src/components/`
   - `Card.tsx`: quitar `shadow-card`; quedarse con `border border-hairline`.
     Ajustar `CardHeader`/`CardBody` a padding uniforme `p-5 lg:p-6`.
   - `Input.tsx` / `Select.tsx`: `rounded-pill` → `rounded-control`.
   - `Modal.tsx`: conserva sombra (sí flota) pero adopta `rounded-card`.
   - `Button.tsx`: se mantiene `rounded-pill`.
3. **Coherencia con el shell** — revisar que `rounded-3xl` (24 px) del shell siga
   leyéndose bien junto al nuevo `rounded-card` (16 px); si choca, subir el shell
   a `rounded-[20px]` como valor único.

### Criterios de aceptación

- Ninguna tarjeta usa sombra para separarse del canvas; usa borde hairline.
- Inputs con radio 8 px y botones con píldora.
- Captura de una pantalla densa (Compras o Inventario) en tema Oscuro y UNET sin
  esquinas incoherentes entre shell y tarjetas.

### Verificación

```bash
npm run typecheck -w apps/admin
npm run test -w apps/admin
npm run build -w apps/admin
```

### Riesgos

- Cambiar `borderRadius.card` afecta **todas** las páginas de golpe; hacerlo en un
  solo commit y revisar visualmente dashboard, POS, KDS y una tabla densa.
- Los tests de componentes (`Input.test`, `Select.test`, `Modal.test`) pueden
  asumir clases; revisarlos si fallan.

### Resultado

- **Tokens** (`tailwind.preset.cjs`): `card` 16px, `inner` 12px, nuevo `control`
  8px, `pill` 9999px.
- **`Card.tsx`**: `border border-hairline bg-surface` (sin `shadow-card`);
  `CardHeader` `px-5 pt-5 lg:px-6 lg:pt-6`; `CardBody` `p-5 lg:p-6`.
- **`Layout.tsx`** (`EmptyState`) y **`Skeleton.tsx`** (`SkeletonCard`): borde
  hairline en vez de sombra.
- **`Input.tsx` / `Select.tsx`**: `rounded-pill` → `rounded-control`.
- **Controles inline** en 7 páginas migrados a `rounded-control` (CuentaPage ×2,
  VentasPage, CajaPage, KardexPage, TomasFisicasPage, ComprasPage, LocalPage ×2).
- **`Modal.tsx`** y **`Button.tsx`** sin cambios (modal flota, botón píldora).
- **Shell**: se mantuvo `rounded-3xl` (24px). Con la tarjeta a 16px la jerarquía es
  correcta (contenedor más redondeado que su contenido); no hubo que bajarlo.
- **`DESIGN.md`** actualizado (§1, §4, §5) con el nuevo vocabulario.
- Verificación: `typecheck`, `test` (18/18) y `build` OK.

---

## 8. V1–V8 — Vidrio global, pastel y dashboard

> **Estado: EJECUTADA.**

### Objetivo

Llevar el vidrio a toda la app, migrar los acentos a pastel (lavanda) y
reestructurar el dashboard completo (KPIs, heatmap, salud de inventario y mermas)
con material de vidrio y color pastel semántico, en ambos temas.

### V1 · Vidrio global
- `theme.css`: tokens `--glass-bg/--glass-alpha/--glass-panel-alpha/--glass-bar-alpha/--glass-edge/--glass-blur`
  por tema; utilidad `.glass-card` unida a `.glass-panel`/`.glass-bar`; fallbacks
  `@supports` y `prefers-reduced-transparency` (caen a `--color-surface`).
- `Card.tsx` pasa a `.glass-card` → propaga a toda la app.
- `Modal.tsx` a `.glass-card` (conserva `shadow-card`). Toast en `main.tsx` a
  surface translúcido + `backdrop-filter`.

### V2 · Pastel + acento lavanda
- Acento `#c9b8f0` (lavanda); se retira violeta `#5433eb` y azul UNET `#003366`.
- Familias pastel con fill + `-ink`: menta, cielo, durazno, rosa, mantequilla;
  token `on-pastel` `#241e3a` para texto sobre fill.
- `Badge.tsx`: tones a `bg-*/25 text-*-ink` (+ tone `butter`).
- `Button.tsx`: `primary` y `danger` usan `text-on-pastel`.
- Reemplazo masivo `text-{success,warning,danger,info}` → `-ink` y
  `text-accent-soft` → `text-accent-ink` (22 archivos).
- Glow ambiental del shell: lavanda + menta.

### V3 · Tema
- `theme.css`: selector `[data-theme='unet']` → `[data-theme='claro']`; `:root` =
  claro.
- `ThemeContext.jsx`: default `'claro'` y migración `'unet'` → `'claro'`.
- `index.html`: `data-theme="claro"` + script anti-flash.
- `AppShell.tsx`: etiqueta "Tema Claro".

### V4–V7 · Dashboard (`DashboardPage.tsx`)
- Nuevo `StatTile` (vidrio, ícono en chip pastel, cifra `tabular-nums`, variante
  héroe con lavado pastel).
- Bento: héroe "Ventas de hoy" + operación (cuentas, reservas, stock, mermas) +
  finanzas (ventas mes, ticket, clientes, productos) + franja de estado de caja.
- Heatmap con rampa pastel, leyenda y celda de pico marcada.
- Salud de inventario: barra apilada por estado + leyenda + críticos tabulares.
- Mermas: pills pastel + barras en durazno.
- Solo datos reales del contrato (`types/src/index.ts:560`).

### Resultado
- Verificación: `typecheck`, `test` (18/18) y `build` OK.
- `DESIGN.md` reescrito (§1–§7) a vidrio + pastel.

### Pendiente tras V
- Cara mono tabular y tope de peso 500 global (tipografía).
- Agrupar la navegación por dominio.
- Revisar `prefers-reduced-motion`.

### Ajustes de layout (posteriores a V)
- KPI: se quitó `row-span` a la tarjeta héroe "Ventas de hoy" (ya no deja espacio
  vacío) y los 10 tiles + caja se reacomodaron en una rejilla de 3 filas × 4
  columnas que llena sin huecos.
- Heatmap: celdas flexibles (`flex-1`) para ocupar el ancho de su tarjeta; ya no
  deja la mitad derecha vacía.
- Panel inferior: rejilla `lg:grid-cols-2 xl:grid-cols-3` con Horas pico, Salud de
  inventario y Mermas en la misma fila (en xl) para no empujarlas hacia abajo.
  Salud de inventario pasó de tabla a lista compacta.
- Shell: se retiró el halo verde inferior derecho; queda solo el lavanda.

---

## 9. S — Sidebar por dominio

> **Estado: EJECUTADA.**

### Objetivo

Pasar de una lista plana de 18 ítems a una navegación agrupada por dominio, con
sub-módulos anidados, y arreglar la navegación móvil.

### Cambios

- **`apps/admin/src/lib/navigation.tsx` (nuevo)**: fuente única.
  - Tipos `NavItem` (`to`, `label`, `icon`, `corto?`, `permiso?`, `admin?`, `end?`,
    `primario?`) y `NavGroup`.
  - `GRUPOS`: 8 dominios (Operación, Salón, Catálogo y almacén, Compras, Dinero,
    Clientes, Marca y contenido, Sistema).
  - `filtrarGrupos(esAdmin, tienePermiso)` filtra por RBAC y oculta grupos vacíos.
  - `esRutaActiva(pathname, item)` respeta `end` para no marcar el padre en sus
    hijos (p. ej. `/inventario` vs `/inventario/kardex`).
- **`AppShell.tsx`**: sidebar con grupos colapsables; el **grupo activo se resalta
  y auto-expande** (`useLocation`); estado persistido en
  `localStorage['licoreria.nav.grupos']`. Ítems anidados con guía vertical.
- **`MobileNavSheet.tsx` (nuevo)**: sheet de vidrio con la nav agrupada completa;
  cierra con Esc o clic en el fondo.
- **Bottom-nav móvil**: 4 primarias (`primario`) + botón "Más" que abre el sheet;
  se acabó el scroll horizontal de 18 ítems.
- **Sub-módulos**: anidados en el sidebar **y** se conservan los `*Tabs.tsx` como
  nav de sección. URLs intactas.

### Resultado
- Verificación: `typecheck`, `test` (18/18) y `build` OK.

### Pendiente tras S
- Tipografía (mono tabular + tope de peso 500).
- Foco atrapado en `MobileNavSheet` (hoy solo Esc/backdrop).
- Command palette ⌘K y breadcrumbs.

---

## 10. N / T / A — Sidebar glass, tipografía y accesibilidad

> **Estado: EJECUTADA.**

### N · Sidebar (replanteo glass)
- Ancho `w-64` → `w-72`; padding `px-4 py-5 lg:px-5`.
- **Tamaño unificado `text-sm` (14px)** para cabeceras de grupo, ítems anidados,
  filas de un solo ítem y controles del pie (mismo tamaño de letra en todo el
  sidebar); la jerarquía queda por peso (cabecera `font-medium`) y por el lavado
  pastel del ítem activo.
- **Activo sin sombra**: `bg-accent/20 text-accent-ink font-medium`; inactivo
  `text-muted hover:bg-ink/5 hover:text-ink`. Se eliminó `shadow-soft`.
- Anidado `ml-3 pl-3 border-l border-hairline/60`.
- Grupos de un solo ítem (Clientes, Sistema) se renderizan como fila directa.
- Footer `text-[15px]` y bloque de usuario `bg-ink/5`.

### T · Tipografía
- `apps/admin/index.html`: Google Fonts con **Inter (400;500;600)** e
  **IBM Plex Mono (400;500)** (con preconnect). Antes Inter no se cargaba y caía a
  `system-ui`.
- `tailwind.preset.cjs`: `fontFamily.mono` = IBM Plex Mono.
- `theme.css`: utilidad `.num` (IBM Plex Mono + `tabular-nums`).
- `.num` aplicado a: `StatTile` y etiquetas del heatmap (Dashboard), celdas
  alineadas a la derecha de `DataTable`, totales de POS/Cuenta y timer de KDS.
- Tope de peso **500**: `font-semibold` → `font-medium` (17 usos en `packages/ui`
  y `apps/admin`).
- `DESIGN.md` §1/§3/§7 actualizado.

### A · Accesibilidad
- Nuevo `packages/ui/src/lib/useFocusTrap.ts` (foco inicial, Tab cíclico, Esc,
  restauración del foco previo), exportado desde `@licoreria/ui`.
- `Modal.tsx` refactorizado para usarlo (sin cambio de comportamiento).
- `MobileNavSheet.tsx`: usa el hook, `tabIndex={-1}` y devuelve el foco al botón
  "Más".

### Resultado
- Verificación: `typecheck`, `test` (18/18) y `build` OK.

### Pendiente (iteración C)
- Command palette ⌘K.
- Breadcrumbs.

---

## 11. R0 — Sesión y acceso por rol

> **Estado: EJECUTADA.** Primera iteración del "reestructurar el flujo por rol".

### Backend
- `rolDominio` añadido a `AuthResponseDto` y `UsuarioActualDto`; lo emite
  `ServicioAutenticacion` (`usuario.Rol.ToString()`), junto al `rol` de seguridad.
- **Seed de un usuario por rol** (`SeedData` + `UsuarioConfiguration` + migración
  `SeedUsuariosPorRol`): Mesero, Barra, Cocina, Host y Editor de contenido
  (`*1@licoreria.com`, contraseña `demo123`), además de Admin y Cajero.
- Matriz rol→permisos (`RolesSeguridad`): **Mesero** recibe `sales:read` y
  `club:read` para ver Cuentas y Mesas.

### Frontend
- `lib/roles.ts`: `rolDominio` → ruta de inicio (`INICIO_POR_ROL`), etiquetas y
  `CUENTAS_DEMO`.
- `AuthContext`: guarda `rolDominio`, expone `inicio` y calcula `esAdmin` por rol
  de dominio.
- `LoginPage`: chips "Entrar como" para las 7 cuentas; tras login aterriza en el
  inicio del rol (o en el destino original).
- `App.tsx`: **guard real** `ContenidoProtegido` que valida el permiso de la ruta
  actual (`itemDeRuta` en `navigation.tsx`) y redirige al inicio del rol si no
  corresponde. Cierra la brecha de acceso por URL.
- `AppShell`: muestra el rol de dominio en el pie.
- `packages/types`: `rolDominio` en `AuthResponse`/`UsuarioActual`.

### Resultado
- Backend `dotnet build` OK; frontend `typecheck`, `test` (18/18) y `build` OK.

### Pendiente (siguientes iteraciones)
- Workspaces por rol (nav curada por rol) y **KDS por área** (`?area=cocina`).
- Módulo IA (frontend), command palette ⌘K y breadcrumbs.
- Reportes y RF con UI pendiente (promociones, catálogo avanzado, CxC, etc.).

---

## 12. R0.1 — Workspaces por rol y módulos restantes

> **Estado: EJECUTADA** (IA omitida por indicación).

### Corrección del login por rol
- El seed de usuarios chocaba con un `mesero1@licoreria.com` creado a mano
  (id distinto): la migración `SeedUsuariosPorRol` fallaba por el índice único de
  correo. Se añadió un `DELETE ... WHERE Email IN (...)` previo al `InsertData` y se
  aplicó la migración a la BD. Ya inician sesión los 7 roles.

### Workspaces por rol
- `navigation.tsx`: `NavGroup.roles` y `NavItem.roles`; `filtrarGrupos(esAdmin,
  tienePermiso, rolDominio)` filtra por permiso **y** rol; `puedeAcceder(...)` para
  el guard de rutas (grupo + ítem + permisos).
- `App.tsx` (`ContenidoProtegido`) usa `puedeAcceder`; se cierra el acceso por URL
  más allá de la curaduría del rol.
- Navegación curada: Operación del Cajero = Dashboard/POS/Cuentas/Ventas/Promociones;
  Mesero = Mesas/Cuentas; Barra/Cocina = KDS; Host = Reservas/Salón/Entradas/VIP;
  Editor = Marca y contenido; Admin = todo.

### KDS por área
- `KdsPage`: área por `?area=barra|cocina` (por defecto según el rol), con toggle y
  suscripción SignalR a la sala del área.

### Navegación fluida
- `Breadcrumbs` (Grupo / Página) dentro del canvas.
- `CommandPalette` (⌘K / Ctrl+K): navegación + acciones rápidas, con foco atrapado.
- Botón "Buscar… ⌘K" en el sidebar.

### Módulos añadidos (sin IA)
- **Reportes** (`/reportes`): ventas, inventario valorizado, compras y propinas.
- **Auditoría** (`/auditoria`): bitácora con filtro por entidad.
- **Promociones** (`/promociones`): CRUD.
- **Cuentas por cobrar** (`/cxc`): registrar y cobrar.
- **Entradas** (`/entradas`): emitir, validar y cancelar.
- **Lista VIP** (`/vip`): CRUD.
- **Catálogo avanzado** (`/catalogos-avanzado`): unidades, impuestos, listas de
  precio y modificadores.
- `packages/types` y `api-client`: DTOs y módulos (`auditoriaApi`,
  `cuentasPorCobrarApi`, `listaVipApi`, `entradasApi`, `unidadesApi`,
  `impuestosApi`, `listasPrecioApi`, `modificadoresApi`; reportes ampliados).

### Resultado
- Backend `dotnet build` OK; migración aplicada; frontend `typecheck`, `test`
  (18/18) y `build` OK.

### Pendiente
- **Módulo IA** (omitido hasta indicación).
- Recetas y códigos de barras por variante.
- Limpieza de docs (drift de nombres en la doc fuente).

### R0.2 — Búsqueda ⌘K y modo de operación (posterior)
- **⌘K con datos**: la paleta busca productos (`catalogoApi.productos`) y clientes
  (`clientesApi.listar`) y navega con `?busqueda=`; POS, Productos y Clientes
  inicializan su búsqueda desde la URL.
- **Modo funcional**: `NavItem.modos` + `filtrarGrupos(..., modo)` y
  `puedeAcceder(..., modo)`; los ítems de discoteca (Mesas, Cuentas, KDS, Reservas,
  Salón, Entradas, VIP) se ocultan en Modo Licorería. El guard cae al primer destino
  permitido si el inicio del rol no aplica en el modo activo.

---

## 13. R1 — Poda al núcleo mínimo

> **Estado: EJECUTADA.**

### Objetivo
Reducir el sistema a un núcleo manejable, quitando redundancia y ocultando (no
borrando) lo secundario, para pulir módulo por módulo.

### N0 · Ocultar secundarios
- `navigation.tsx`: `NavGroup.oculto` / `NavItem.oculto`. `filtrarGrupos` los
  excluye y `puedeAcceder` los deniega (si se entra por URL, redirige al primer
  destino visible). Reversible quitando el flag.
- Ocultos: grupos Compras, Clientes (incl. CxC), Marca y contenido, Analítica
  (Reportes/Auditoría); ítems Promociones, Entradas, Lista VIP, Tesorería.

### N1 · Retirar Modo Licorería/Discoteca
- Quitado el toggle en `AppShell` (sidebar y header) y el filtro `modos` de
  `navigation`/guard. `ModoProvider` retirado de `main.tsx`. El acceso queda regido
  por **rol + permisos**.

### N2 · Catálogo unificado
- `CatalogoTabs` (Productos · Categorías y marcas · Unidades/impuestos/listas) en
  las tres pantallas; un solo ítem de nav "Catálogo" → `/productos`.

### N3 · Salón unificado
- Un solo ítem de nav "Salón" → `/salon`, con `SalonTabs` (Planos · Zonas y mesas).

### N4 · Reportes/CxC
- Reportes/Auditoría y Clientes/CxC quedan ocultos; el Dashboard enlaza a Reportes
  para el Administrador.

### Resultado
- `typecheck`, `build` y `test` OK.
- Núcleo visible: Operación, Salón, Catálogo y almacén, Dinero, Sistema.

---

## 14. M — Editor de mapa del local (2D tipo juego)

> **Estado: MVP EJECUTADO.**

### Concepto
Mapa 2D top-down editable: paleta de elementos, zonas como regiones de color,
rejilla con snap, y el mismo mapa para operar en vivo. Todo **SVG vectorial**
(sin assets externos).

### Modelo (una sola fuente espacial)
- `plano`: `anchoFondo`, `altoFondo`, `rejilla`, `piso`.
- `plano_elemento`: `forma`, `color`, `z`, `mesaId` (enlaza la mesa operativa).
- `zona`: `color` + región (`posX/posY/ancho/alto`).
- Migración `PlanoMapaEditor` aplicada; DTOs/validators/servicio actualizados.

### Frontend
- `components/mapa/elementos.tsx`: catálogo (`ELEMENTOS`) con dibujos SVG propios
  (mesas con sillas, barra con taburetes, pista a cuadros, escenario, DJ, muro,
  columna, escalera, entrada, baños, guardarropía, caja, planta).
- `components/mapa/MapaView.tsx`: render compartido (piso, rejilla, zonas como
  regiones, elementos ordenados por `z`, estados de mesa).
- `PlanosPage`: editor con paleta, mover con **snap**, redimensionar, rotar,
  capas (z), duplicar, borrar, pan/zoom, **undo/redo** e inspector (forma, color,
  etiqueta, rotación, tamaño, zona, enlace a mesa).
- `ZonasMesasPage`: color + región de zona.
- `PlanoPage` (operación): usa `MapaView` con estados por mesa (Libre/Ocupada/
  Reservada/En limpieza) y fallback sintético desde las mesas si no hay mapa.

### Resultado
- Backend `dotnet build` + migración OK; frontend `typecheck`/`build`/`test` OK.

### Post-MVP
- Zonas poligonales, varios pisos, plantillas, importar croquis (IA), sprites.

### Optimización del editor (post-MVP)
- **Arrastre imperativo**: al mover un elemento se actualiza su `transform` y la
  posición del overlay directamente en el DOM durante el `pointermove` (sin
  re-render); el estado se confirma una sola vez en `pointerup`. Elimina el lag y
  el desfase respecto al cursor.
- **Resize/rotar** con estado limitado a **un update por frame** (`requestAnimationFrame`).
- `MapaView` memoizado y elementos en `ElementoMapa` memoizados; piso/grid y zonas
  separados para que solo se re-renderice lo que cambia.
- Handler de puntero **estable** para no romper la memoización.
