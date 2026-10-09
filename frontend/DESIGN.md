# Design System · Licorería / Discoteca (Fase 5)

Dirección visual: **superficies sólidas semánticas sobre shadcn/ui**, con un
acento de marca **lavanda** y **familias pastel de estado**. Convencional y
operativo, con personalidad propia: tipografía Inter con tracking negativo,
cifras en IBM Plex Mono y acento lavanda en acción y navegación.

> Este documento es la fuente de verdad visual. Los tokens viven en
> `packages/config/tailwind.preset.cjs` y `packages/ui/src/styles/theme.css`.
> Los componentes son **shadcn/ui vendoreado** (primitivas Radix) en
> `packages/ui/src/components/ui/`, re-expuestos por `@licoreria/ui`.
> Bitácora de la migración: `frontend/docs/rediseno-convencional.md`.

---

## 1. Principios

1. **Superficie sólida, no vidrio.** Fondo de lienzo gris, tarjetas blancas,
   bordes hairline y sombras discretas. Sin `backdrop-filter` ni halos.
2. **Radios por función.** Controles de formulario a `8px`, tarjetas a `12px`
   (`rounded-xl`), botones `rounded-md`; chips y badges a píldora `9999px`.
3. **Acento lavanda `#c9b8f0` = marca y acción.** Botones primarios, foco de
   teclado (`ring`), ítem activo de navegación (lavado `bg-primary/15`) y
   marca. Texto sobre el fill: `on-pastel` `#241e3a`.
4. **Color pastel semántico.** Cada familia tiene un *fill* pastel y un texto
   legible sobre él (`-fg`): menta (éxito), cielo (info), durazno (atención),
   rosa (alerta), mantequilla (caja). El color significa, no decora.
5. **Tipografía por tracking y jerarquía.** Inter (400;500;600) con tracking
   negativo; peso máximo **500**. Cifras en **IBM Plex Mono** con
   `tabular-nums` (utilidad `.num`).
6. **Mobile First** y zonas táctiles ≥ 44px.

## 2. Tokens de color

### Neutros (esquema shadcn/ui)

| Token | Claro | Oscuro | Rol |
| :--- | :--- | :--- | :--- |
| `--background` | `#f2f4f5` | `#0b0d10` | Lienzo de página |
| `--card` / `--popover` | `#ffffff` | `#1b1f26` | Superficies base y flotantes |
| `--foreground` | `#0b0d10` | `#f5f7fa` | Texto primario |
| `--muted` | `#ebebeb` | `#262b33` | Superficie tenue / hover |
| `--muted-foreground` | `#787574` | `#9aa3b2` | Texto secundario, labels |
| `--border` / `--input` | `#e0e0e0` / `#ebebeb` | `#262b33` | Bordes y controles |
| `--ring` | `#c9b8f0` | `#c9b8f0` | Foco de teclado |

### Acento y familias pastel

| Rol | Fill | Texto sobre fill (`-fg`) |
| :--- | :--- | :--- |
| Marca / acción (lavanda) | `#c9b8f0` | `on-pastel` `#241e3a` |
| Éxito (menta) | `#a8e6cf` | `#1f5c46` |
| Info (cielo) | `#a9d6f5` | `#1e4c6e` |
| Atención (durazno) | `#f7c9a6` | `#7a4318` |
| Alerta (rosa) | `#f3b6c4` | `#7a2436` |
| Caja (mantequilla) | `#f4e1a1` | `#6b5714` |

> Los `-fg` son oscuros y legibles sobre el fill pastel **en ambos temas**.
> `ThemeContext` alterna `data-theme="claro"` / `"dark"` en `<html>`. Por
> defecto: **claro**.

## 3. Tipografía

- Familia UI: `Inter` (Google Fonts), `system-ui` como fallback.
- Familia de cifras: `IBM Plex Mono` (dinero, cantidades, SKU, timers) con
  `tabular-nums`; utilidad `.num`.
- Escala: 11px caption · 12px body-sm · 14px body · 16px body-lg · 20px display.
- Peso máximo **500** (`font-medium`); jerarquía por tamaño, color y tracking.

## 4. Radios, sombras y espaciado

- Radios: controles `8px`, tarjetas `12px` (`rounded-xl`), botones `rounded-md`,
  chips/badges píldora `9999px`.
- Sombras: `shadow-sm` (controles), `shadow-md` (menús/modales), `shadow-lg`
  (diálogos). Las tarjetas usan `shadow-sm` + borde hairline.
- Espaciado base (px): 4, 6, 8, 10, 12, 16, 20, 24, 32, 40, 48, 64.
- Layout: `max-width 1600px`; shell con sidebar a la izquierda `w-72` (colapsable a rail de iconos `w-16`), topbar `h-16` con buscador a la izquierda (20px del sidebar) y canvas `bg-background` con padding 16/24px.
- Marca: **CORCHO** — corcho SVG + nombre en la cabecera del sidebar (y login); iconos de nav en `text-accent-ink`.

## 5. Componentes

| Componente | Especificación |
| :--- | :--- |
| `Card` | 12px, `bg-card`, borde `border`, `shadow-sm`, padding `p-5 lg:p-6` |
| `Button` (primary) | `rounded-md`, lavanda `bg-primary`, texto `on-pastel` |
| `Button` (outline/ghost) | `rounded-md`, borde hairline, hover lavanda |
| `Input` / `Select` | radio 8px, `bg-background`, borde `input`, `ring-ring` |
| `Buscador` | input de búsqueda con lupa a la izquierda y limpiar (patrón de listado) |
| `FiltroDropdown` | label arriba + botón con menú Radix (patrón de listado) |
| `FiltroRango` | label arriba + botón que abre popover con Mín/Máx (patrón de listado) |
| `FiltroFechas` | label "Fecha" arriba + botón que abre popover con Desde/Hasta (patrón de listado) |
| `LimpiarFiltros` | botón para limpiar todos los filtros activos (patrón de listado) |
| `Badge` / `Pill` | píldora, fill pastel + texto `-fg` |
| `StatusBadge` | familia pastel por estado (ver mapeo §6) |
| `SidebarNav` | rail 288px (`w-72`) colapsable a `w-16` (solo iconos del módulo), iconos en `text-accent-ink`, ítem activo `bg-primary/15` + `font-medium` |
| `DataTable` | tabla shadcn, filas separadas por hairline, hover `bg-muted/50` |
| `Modal` / `Dialog` | Radix, `rounded-xl`, `bg-card`, `shadow-lg` |
| `DropdownMenu` | Radix, `bg-popover`, `shadow-md` |
| `Sheet` (nav móvil) | Radix, desliza desde abajo, `rounded-t-2xl` |
| `Command` (⌘K) | cmdk, diálogo con búsqueda y lista |
| `Alert` | pastel al 20% + texto `-fg` |
| `Empty` | borde discontinuo, ícono en chip |
| `Skeleton` | `bg-muted` con pulse |

## 6. Mapeo de estados (operación)

| Estado | Familia pastel |
| :--- | :--- |
| Mesa Libre | Menta (success) |
| Mesa Reservada | Durazno (warning) |
| Mesa Ocupada | Rosa (danger) |
| Mesa En limpieza | Cielo (info) |
| Comanda Recibido | Cielo (info) |
| Comanda Preparado | Menta (success) |
| Comanda Entregado | Muted |

## 7. Do / Don't

**Do**
- Usar los tokens semánticos y los componentes de `@licoreria/ui` (shadcn).
- Un solo acento (lavanda) para marca y acción; pastel solo semántico.
- Cifras con `.num` (mono tabular); tracking negativo en titulares.
- Tarjetas con borde hairline + `shadow-sm`; hover discreto `bg-muted/50`.
- `FieldGroup`/`Field`-like en formularios: label + control + mensaje.

**Don't**
- No reintroducir vidrio (`backdrop-filter`, halos, `.glass-*`), violeta
  saturado `#5433eb` ni azul UNET `#003366`.
- No usar pesos > 500 (`font-medium` como tope).
- No usar pastel como texto sin la variante `-fg` (rompe contraste).
- No colores en crudo (`bg-blue-500`): siempre token semántico.
- No `space-x/y-*`: usar `flex` + `gap-*`.

---

## 7.5 Patrón de listado (buscador + filtros)

**Toda pantalla de listado/CRUD sigue un único layout.** Un `<Card>` que
engloba todo: cabecera con búsqueda y filtros, luego la tabla.

```
<Card>
  <CardHeader className="flex flex-col items-stretch gap-3">
    ── Fila 1 ────────────────────────────────────────────────
    [ Buscador (izq) ]  [ FiltroDropdown general ]  [ FiltroFechas ]  [ ＋ Añadir (der) ]
    ── hairline: <div className="border-b border-border" /> ──
    ── Fila 2 ────────────────────────────────────────────────
    [ FiltroDropdown concreto ]  [ FiltroRango (popover) ]  [ FiltroFechas (popover) ]  [ Limpiar (der) ]
  </CardHeader>
  <CardBody>
    <DataTable … /> <Pagination … />
  </CardBody>
</Card>
```

Cada filtro (botón desplegable o popover) lleva su **label arriba** y muestra
**solo el valor seleccionado** dentro del botón.

### Componentes comunes

| Componente | Rol | Especificación |
| :--- | :--- | :--- |
| `Buscador` | Input con **lupa a la izquierda** y botón X para limpiar a la derecha | `w-80 shrink-0`, `onCambio(valor)` |
| `FiltroDropdown` | **Label arriba** + botón con el valor seleccionado y chevron que abre menú Radix | panel sólido `bg-popover rounded-xl border shadow-md` |
| `FiltroRango` | **Label arriba** + botón que abre un **popover** con inputs Mín/Máx | `onMinimo`/`onMaximo`, valor "5 – 20" |
| `FiltroFechas` | **Label "Fecha" arriba** + botón que abre un **popover** con Desde/Hasta | `onDesde`/`onHasta`, valor "dd/mm/aaaa – …" |
| `FiltroPopover` | Base: **label arriba** + botón + panel posicionado bajo él, cierra con clic fuera/Escape | `bg-popover rounded-xl border shadow-md` |
| `LimpiarFiltros` | Botón ghost con icono para limpiar **todos** los filtros y el buscador | `disabled` si no hay filtros activos |
| Hairline | `<div className="border-b border-border" />` | separa fila 1 de fila 2 |

### Reglas

- **El label del filtro va arriba del botón** (`text-xs font-medium text-foreground`);
  el botón muestra solo el valor seleccionado ("Todos", "Ron", "5 – 20", rango
  de fechas…). Los botones de filtro usan radio `rounded` (menos redondeado).
- **Buscador a la izquierda, botón de añadir a la derecha** (fila 1).
- **Filtros generales en fila 1** (estado, tipo, **rango de fechas** como
  `FiltroFechas`) y **filtros concretos en fila 2** (categoría, marca, precio,
  rol, motivo…).
- **Botón `LimpiarFiltros` al final de la fila de filtros** (fila 2, o fila 1 si
  no hay fila 2), alineado a la derecha con `ml-auto`. Resetea **filtros +
  buscador** y vuelve a página 1.
- **Sin píldoras como filtros**: se usan `FiltroDropdown` (menú Radix) con
  panel sólido redondeado que agrupa las opciones.
- **Los rangos y las fechas se colocan desde un popover** (`FiltroRango`,
  `FiltroFechas`) para no mostrar dos inputs confusos en línea.
- **Filtrado server-side** cuando la API lo soporta (re-query con params).
- **Filtrado client-side** cuando no: la query amplía `pageSize`
  (`PAGE_SIZE_FILTRO_LOCAL = 500`) y se pagina en memoria
  (`paginarEnMemoria` en `apps/admin/src/lib/filtros.ts`).
- **Formularios de crear/editar en `Modal`** (no inline expandido): patrón
  tabla + botón añadir. Las ediciones rápidas en fila (`expandedRow`) se
  reservan para acciones (devolución, pago, ajuste).

---

## 8. Web pública · tema "Hungry Tiger"

El sitio público (`apps/public-web`) **no** usa el tema operativo: es un póster
tipográfico de inspiración *spice-label* (paleta dorado-sobre-óxido, `Antonio`
en titulares, vitrina 3D con React Three Fiber). No se tocó en esta fase.