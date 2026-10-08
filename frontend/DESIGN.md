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
- Layout: `max-width 1600px`; shell con sidebar fija `w-72`, topbar `h-14`
  sticky y canvas `bg-background` con padding 16/24px.

## 5. Componentes

| Componente | Especificación |
| :--- | :--- |
| `Card` | 12px, `bg-card`, borde `border`, `shadow-sm`, padding `p-5 lg:p-6` |
| `Button` (primary) | `rounded-md`, lavanda `bg-primary`, texto `on-pastel` |
| `Button` (outline/ghost) | `rounded-md`, borde hairline, hover lavanda |
| `Input` / `Select` | radio 8px, `bg-background`, borde `input`, `ring-ring` |
| `Badge` / `Pill` | píldora, fill pastel + texto `-fg` |
| `StatusBadge` | familia pastel por estado (ver mapeo §6) |
| `SidebarNav` | rail 288px, ítem activo `bg-primary/15` + `font-medium` |
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

## 8. Web pública · tema "Hungry Tiger"

El sitio público (`apps/public-web`) **no** usa el tema operativo: es un póster
tipográfico de inspiración *spice-label* (paleta dorado-sobre-óxido, `Antonio`
en titulares, vitrina 3D con React Three Fiber). No se tocó en esta fase.