# Design System · Licorería / Discoteca (Fase 4)

Dirección visual: **vidrio opaco (glass) + acentos pastel**. Superficies tipo
"vidrio liso" translúcido con canto de 1px, sobre un sistema de color pastel
semántico. Dos temas: **"Claro"** (por defecto) y **"Oscuro"**, ambos con el mismo
material de vidrio.

> Este documento es la fuente de verdad visual. Los tokens viven en
> `packages/config/tailwind.preset.cjs` y `packages/ui/src/styles/theme.css`.
> Bitácora de la migración: `frontend/docs/rediseno-shell-glass.md`.

---

## 1. Principios

1. **Vidrio opaco como material único.** Tarjetas, modales, toasts y el shell usan
   `.glass-card` / `.glass-panel` / `.glass-bar`: fondo blanco translúcido + blur,
   borde hairline y canto interior de 1px que da "peso". Sin sombras de elevación
   (salvo lo que de verdad flota: modales, menús).
2. **Radios por función.** Botones, chips y filtros a píldora `9999px`; inputs y
   selects a `8px`; tarjetas a `16px`; medios internos a `12px`; paneles del shell
   a `24px`.
3. **Acento pastel.** Lavanda `#c9b8f0` como marca y acción (texto `on-pastel`
   `#241e3a` sobre el fill). Sin acentos saturados.
4. **Color pastel semántico.** Cada familia tiene un *fill* pastel y una variante
   de *texto* (`-ink`) legible en el tema activo. El color significa, no decora.
5. **Tipografía por tracking y jerarquía, no por peso.** Inter (Google Fonts) con
   tracking negativo; peso máximo **500**. Cifras (dinero, cantidades, códigos) en
   **IBM Plex Mono** con `tabular-nums` (utilidad `.num`).
6. **Mobile First** y zonas táctiles ≥ 44px (operación en tablet/teléfono).

## 2. Tokens de color

### Neutros

| Nombre | Claro | Oscuro | Rol |
| :--- | :--- | :--- | :--- |
| Canvas | `#f2f4f5` | `#0b0d10` | Fondo de página |
| Surface | `#ffffff` | `#14171c` | Superficie base |
| Elevated | `#ffffff` | `#1b1f26` | Superficies anidadas |
| Hairline | `#ebebeb` | `#262b33` | Divisores 1px |
| Ink | `#0b0d10` | `#f5f7fa` | Texto primario |
| Muted | `#787574` | `#9aa3b2` | Texto secundario, labels |
| Stone | `#cccccc` | `#3a4150` | Placeholders, deshabilitado |

### Acento y familias pastel

Cada familia tiene un **fill** pastel y una variante de **texto** (`-ink`) legible
en cada tema.

| Rol | Fill | Texto `-ink` claro | Texto `-ink` oscuro |
| :--- | :--- | :--- | :--- |
| Acento / marca (lavanda) | `#c9b8f0` | `#6b4fc9` | `#e3dbfa` |
| Éxito (menta) | `#a8e6cf` | `#1f5c46` | `#bef0db` |
| Info (cielo) | `#a9d6f5` | `#1e4c6e` | `#c5e4fa` |
| Atención (durazno) | `#f7c9a6` | `#7a4318` | `#fbdcc3` |
| Alerta (rosa) | `#f3b6c4` | `#7a2436` | `#f8cdd6` |
| Caja (mantequilla) | `#f4e1a1` | `#6b5714` | `#f8ecbb` |

> Texto sobre un fill pastel: token `on-pastel` `#241e3a` (siempre oscuro).
> `ThemeContext` alterna `data-theme="claro"` / `"dark"` en `<html>`. Por defecto:
> **claro**. El azul UNET `#003366` quedó retirado.

### Material de vidrio

| Token | Claro | Oscuro |
| :--- | :--- | :--- |
| `--glass-bg` | `#ffffff` | `#ffffff` |
| `--glass-alpha` (card) | `0.72` | `0.07` |
| `--glass-panel-alpha` | `0.62` | `0.05` |
| `--glass-bar-alpha` | `0.82` | `0.09` |
| `--glass-edge` | `inset 0 1px 0 #fff` | `inset 0 1px 0 rgb(255 255 255 / 0.10)` |
| `--glass-blur` | `20px` | `24px` |

## 3. Tipografía

- Familia UI: `Inter` (cargada por Google Fonts), `system-ui` como fallback.
- Familia de cifras: `IBM Plex Mono` (dinero, cantidades, SKU, timers) con
  `tabular-nums`; utilidad `.num`.
- Rango: 11px caption · 12px body-sm · 14px body · 16px body-lg · 20px display.
- Peso máximo **500** (`font-medium`); la jerarquía se logra por tamaño, color y
  tracking, no por negrita.

| Rol | Tamaño | Peso | Tracking |
| :--- | :--- | :--- | :--- |
| Display | 20px | 600 | -1.0px |
| Título sección | 16px | 600 | -0.5px |
| Body | 14–16px | 400 | -0.3px |
| Meta / caption | 11–12px | 500 | -0.2px |

## 4. Radios, sombras y espaciado

- Radios: tarjetas `16px`, medios internos `12px`, controles de formulario `8px`,
  botones/chips/píldoras `9999px`. El shell del admin (`AppShell`) usa `rounded-3xl`
  (24px) para los paneles de vidrio.
- Sombras: reservadas a lo que flota sobre el contenido (modales, menús, glow de
  marca). Las tarjetas de vidrio **no** usan sombra; se delimitan con borde hairline
  y su canto interior.
  - `shadow-card`: `rgba(0,0,0,0.1) 0 4px 24px` (claro) — solo Modal/menús.
  - `shadow-glow`: `rgba(107,79,201,0.18) 0 8px 30px` (claro) — marca/acción.
  - `shadow-soft`: `rgba(0,0,0,0.25) 0 2px 8px 0` (ítem activo de navegación).
- Superficies de vidrio (toda la app): `.glass-card`, `.glass-panel` y `.glass-bar`
  en `packages/ui/src/styles/theme.css`; blanco translúcido + `backdrop-blur`,
  borde `ink/6%` y canto superior. Fallbacks `@supports` y
  `prefers-reduced-transparency`.
- Espaciado base (px): 4, 6, 8, 10, 12, 16, 20, 24, 32, 40, 48, 64.
- Layout: `max-width 1600px`, cota del shell 16px (móvil) / 24px (escritorio),
  padding de tarjeta 20px / 24px.

## 5. Componentes

| Componente | Especificación |
| :--- | :--- |
| `Card` | 16px, vidrio (`.glass-card`), canto 1px, sin sombra; padding `p-5 lg:p-6` |
| `Button` (primary) | píldora, lavanda `#c9b8f0`, texto `on-pastel`, `shadow-glow` |
| `Button` (ghost) | píldora, transparente, borde hairline |
| `Input` / `SearchBar` | radio 8px, vidrio, 1px hairline, padding-left 20px |
| `Pill` / `StatusChip` | píldora, fill pastel al 25% + texto `-ink` |
| `StateBadge` | familia pastel por estado de mesa/comanda (ver tokens semánticos) |
| `SidebarNav` | rail 256px en escritorio, bottom-nav flotante en móvil; ítem activo con fondo elevado y radio 16px |
| `StatTile` | tile de KPI: vidrio, ícono en chip pastel, cifra `tabular-nums`; variante héroe con lavado pastel |
| `Table` | vidrio, filas separadas por hairline, scroll horizontal (`overflow-x-auto`) |
| `Skeleton` | vidrio con animación pulse |
| `Toast` | vidrio, `title` + `detail` RFC 7807 |
| `Modal` | vidrio sobre backdrop oscuro, radio 16px, conserva sombra (sí flota) |

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
- Usar vidrio (`.glass-card`) en toda superficie; el canto 1px da el peso.
- Un solo acento pastel (lavanda) para marca y acción.
- Separar superficies con borde hairline/canto, no con sombras.
- Color pastel con intención semántica; cifras con `tabular-nums`.
- Mantener 64px de aire entre secciones.

**Don't**
- No reintroducir violeta saturado `#5433eb` ni azul UNET `#003366`.
- No usar pesos > 500: el tope es `font-medium`.
- No esquinas rectas en tarjetas/botones/(inputs a 8px).
- No usar pastel como texto sin la variante `-ink` (rompe contraste).
- No gradientes decorativos: el color lo dan los tokens semánticos.

---

## 8. Web pública · tema "Hungry Tiger"

El sitio público (`apps/public-web`) **no** usa el tema operativo de arriba: es un
póster tipográfico de inspiración *spice-label*. Aquí está el resumen; los tokens
viven en `apps/public-web/tailwind.config.cjs` y el remapeo de variables en
`apps/public-web/src/styles/index.css`.

- **Paleta dorado-sobre-óxido:** Ember Rust `#823513` (lienzo), Dark Spice `#402011`
  (tarjetas), Charred Clove `#281006` (fondo profundo), Tiger Gold `#faae33`
  (texto/acento), Cardamom Brown `#6b2e12` (bordes), Chili Red `#d1255c` (solo
  badges de picante). Sin blancos, azules ni neutros fríos.
- **Tipografía póster:** `Antonio` (sustituto de Salmond) para titulares a
  `clamp(3.75rem, 12.5vw, 12.1875rem)`, `line-height 0.82`, tracking negativo;
  `Inter` para microcopy y labels.
- **Formas:** botones/badges/inputs píldora (`9999px`); tarjetas `6px`. Sin sombras:
  la profundidad viene del salto entre los tres marrones.
- **Ritmo:** reglas punteadas doradas entre secciones y marcas de agua botánicas en
  SVG (`.botanical-layer`, opacidad 6%).
- **Producto:** vitrina 3D de la botella de vino tinto (glTF en
  `public/models/vino-tinto/`) renderizada con React Three Fiber, **sin marco ni
  tarjeta**, iluminada con luces cálidas. El chunk de `three` se carga de forma
  diferida (`React.lazy`) para no pesar en el primer render.
- **Atribución:** modelo «Vino Tinto Castaño Colección» · *anaa_ggarcia* · CC BY 4.0
  (acreditado en el pie de página).
