# Design System · Licorería / Discoteca (Fase 4)

Patrón de referencia: **"Shop — Floating shopping constellation on white marble"**,
adaptado a una **tema oscuro** de operación (POS, KDS, mesas) con el **Azul UNET
`#003366`** como acento institucional y el **violeta `#5433eb`** como acento de acción.

> Este documento es la fuente de verdad visual. Los tokens viven en
> `packages/config/tailwind.preset.js` y `packages/ui/src/styles/theme.css`.

---

## 1. Principios

1. **Superficies suaves y elevadas.** Tarjetas con radio 28px y sombra de doble capa.
   Nunca bordes visibles en tarjetas: la separación la hace la sombra y el cambio de
   superficie.
2. **Controles tipo píldora.** Inputs, botones, chips y barras de búsqueda a `9999px`.
3. **Un solo acento saturado.** Violeta `#5433eb` para acción/identidad; azul UNET
   `#003366` como acento alterno. Nada más saturado.
4. **Tipografía por tracking, no por peso.** Inter (sustituto de GT Standard) con
   tracking negativo; máx. peso 600, nunca 700+.
5. **Densidad compacta con aire vertical.** Gaps de 12px dentro de tarjetas; 64px entre
   secciones.
6. **Mobile First** y zonas táctiles ≥ 44px (operación en tablet/teléfono).

## 2. Tokens de color

### Tema oscuro (por defecto en `admin`)

| Nombre | Valor | Rol |
| :--- | :--- | :--- |
| Canvas | `#0b0d10` | Fondo de página |
| Surface | `#14171c` | Tarjetas, sidebar, inputs |
| Surface Elevated | `#1b1f26` | Tarjetas flotantes / modales |
| Hairline | `#262b33` | Divisor 1px, anillos de avatar |
| Ink | `#f5f7fa` | Texto primario |
| Muted | `#9aa3b2` | Texto secundario, labels |
| Cool Stone | `#3a4150` | Placeholders, deshabilitados |
| Shop Violet | `#5433eb` | Acción primaria / marca |
| Violet Wash | `#c0b5f3` | Halo translúcido tras el botón |
| UNET Blue | `#003366` | Acento institucional / tema "Azul UNET" |
| Success | `#2fbf71` | Libre / Preparado |
| Warning | `#f5a524` | Reservada / stock bajo |
| Danger | `#ef4444` | Ocupada / error |
| Info | `#3b82f6` | En limpieza / info |

### Tema "Azul UNET" (claro institucional)

| Nombre | Valor |
| :--- | :--- |
| Canvas | `#f2f4f5` |
| Surface | `#ffffff` |
| Ink | `#0b0d10` |
| Muted | `#787574` |
| Hairline | `#ebebeb` |
| Accent | `#003366` |
| Accent hover | `#004b8f` |

> `ThemeContext` alterna `data-theme="dark"` / `data-theme="unet"` en `<html>`.
> Por defecto: **oscuro**.

## 3. Tipografía

- Familia: `Inter` (sustituto de GT Standard), `system-ui` como fallback.
- Rango: 11px caption · 12px body-sm · 14px body · 16px body-lg · 20px display.

| Rol | Tamaño | Peso | Tracking |
| :--- | :--- | :--- | :--- |
| Display | 20px | 600 | -1.0px |
| Título sección | 16px | 600 | -0.5px |
| Body | 14–16px | 400 | -0.3px |
| Meta / caption | 11–12px | 500 | -0.2px |

## 4. Radios, sombras y espaciado

- Radios: tarjetas `28px`, imágenes internas `20px`, píldoras/inputs/botones `9999px`,
  chips `9999px`.
- Sombras:
  - `shadow-card`: `rgba(0,0,0,0.35) 0 4px 24px 0` (oscuro) — adaptar opacidad.
  - `shadow-violet`: `rgba(69,36,219,0.34) 0 4px 24px 0`.
  - `shadow-soft`: `rgba(0,0,0,0.25) 0 2px 8px 0`.
- Espaciado base (px): 4, 6, 8, 10, 12, 16, 20, 24, 32, 40, 48, 64.
- Layout: `max-width 1200px`, gap de sección 64px, gap de elementos 12px.

## 5. Componentes

| Componente | Especificación |
| :--- | :--- |
| `Card` | 28px, surface, `shadow-card`, sin borde, padding 0 (imagen bleed) o 16px |
| `Button` (primary) | píldora, violeta, sombra violeta, texto 14px |
| `Button` (ghost) | píldora, transparente, borde hairline |
| `Input` / `SearchBar` | píldora, surface, 1px hairline, padding-left 20px, reserva 48px para submit |
| `Pill` / `StatusChip` | píldora, hairline, sombra suave |
| `StateBadge` | color por estado de mesa/comanda (ver tokens semánticos) |
| `SidebarNav` | rail 64px en desktop, bottom-nav en móvil; ítem activo con fondo surface elevado y radio 20px |
| `Table` | surface, filas separadas por hairline, scroll horizontal (`overflow-x-auto`) |
| `Skeleton` | surface elevado con animación pulse |
| `Toast` | surface elevado, borde izquierdo por severidad, `title` + `detail` RFC 7807 |

## 6. Mapeo de estados (operación)

| Estado | Color |
| :--- | :--- |
| Mesa Libre | Success |
| Mesa Reservada | Warning |
| Mesa Ocupada | Danger |
| Mesa En limpieza | Info |
| Comanda Recibido | Info |
| Comanda Preparado | Success |
| Comanda Entregado | Muted |

## 7. Do / Don't

**Do**
- Radio 28px en tarjetas y 9999px en controles.
- Un solo acento saturado; el violeta es marca + acción.
- Separar superficies con sombra, no con bordes.
- 64px de aire entre secciones.

**Don't**
- No usar pesos ≥ 700.
- No esquinas rectas en tarjetas/botones/inputs.
- No bordes en tarjetas elevadas.
- No gradientes/ilustraciones decorativas: las imágenes de producto dan el color.
