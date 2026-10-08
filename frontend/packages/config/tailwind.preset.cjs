/**
 * Preset compartido de Tailwind para el frontend de Licorería / Discoteca.
 *
 * Esquema semántico de shadcn/ui (background, card, primary, muted, border,
 * ring, destructive…) + familias pastel de estado (success/warning/info/butter).
 * Los colores apuntan a variables CSS (packages/ui/src/styles/theme.css) para
 * soportar los temas "Claro" y "Oscuro" en caliente.
 *
 * Se conservan aliases legacy (canvas, ink, accent-ink, on-pastel…) para la
 * migración incremental; se retirarán cuando no queden usos.
 */
module.exports = {
  theme: {
    extend: {
      colors: {
        /* — Esquema canónico (shadcn/ui) — */
        background: 'rgb(var(--background) / <alpha-value>)',
        foreground: 'rgb(var(--foreground) / <alpha-value>)',
        card: 'rgb(var(--card) / <alpha-value>)',
        'card-foreground': 'rgb(var(--card-foreground) / <alpha-value>)',
        popover: 'rgb(var(--popover) / <alpha-value>)',
        'popover-foreground': 'rgb(var(--popover-foreground) / <alpha-value>)',
        primary: 'rgb(var(--primary) / <alpha-value>)',
        'primary-foreground': 'rgb(var(--primary-foreground) / <alpha-value>)',
        secondary: 'rgb(var(--secondary) / <alpha-value>)',
        'secondary-foreground': 'rgb(var(--secondary-foreground) / <alpha-value>)',
        muted: 'rgb(var(--muted) / <alpha-value>)',
        'muted-foreground': 'rgb(var(--muted-foreground) / <alpha-value>)',
        accent: 'rgb(var(--accent) / <alpha-value>)',
        'accent-foreground': 'rgb(var(--accent-foreground) / <alpha-value>)',
        destructive: 'rgb(var(--destructive) / <alpha-value>)',
        'destructive-foreground': 'rgb(var(--destructive-foreground) / <alpha-value>)',
        border: 'rgb(var(--border) / <alpha-value>)',
        input: 'rgb(var(--input) / <alpha-value>)',
        ring: 'rgb(var(--ring) / <alpha-value>)',
        /* Familias pastel de estado. */
        success: 'rgb(var(--success) / <alpha-value>)',
        'success-foreground': 'rgb(var(--success-foreground) / <alpha-value>)',
        warning: 'rgb(var(--warning) / <alpha-value>)',
        'warning-foreground': 'rgb(var(--warning-foreground) / <alpha-value>)',
        info: 'rgb(var(--info) / <alpha-value>)',
        'info-foreground': 'rgb(var(--info-foreground) / <alpha-value>)',
        butter: 'rgb(var(--butter) / <alpha-value>)',
        'butter-foreground': 'rgb(var(--butter-foreground) / <alpha-value>)',

        /* — Aliases legacy (migración incremental; retirar al final) — */
        canvas: 'rgb(var(--color-canvas) / <alpha-value>)',
        surface: 'rgb(var(--color-surface) / <alpha-value>)',
        elevated: 'rgb(var(--color-elevated) / <alpha-value>)',
        hairline: 'rgb(var(--color-hairline) / <alpha-value>)',
        ink: 'rgb(var(--color-ink) / <alpha-value>)',
        stone: 'rgb(var(--color-stone) / <alpha-value>)',
        'accent-soft': 'rgb(var(--color-accent-soft) / <alpha-value>)',
        'accent-ink': 'rgb(var(--color-accent-ink) / <alpha-value>)',
        violet: 'rgb(var(--color-violet) / <alpha-value>)',
        'success-ink': 'rgb(var(--color-success-ink) / <alpha-value>)',
        'warning-ink': 'rgb(var(--color-warning-ink) / <alpha-value>)',
        danger: 'rgb(var(--color-danger) / <alpha-value>)',
        'danger-ink': 'rgb(var(--color-danger-ink) / <alpha-value>)',
        'info-ink': 'rgb(var(--color-info-ink) / <alpha-value>)',
        'butter-ink': 'rgb(var(--color-butter-ink) / <alpha-value>)',
        'on-pastel': 'rgb(var(--color-on-pastel) / <alpha-value>)',
      },
      borderRadius: {
        sm: '6px',
        DEFAULT: '8px',
        md: '12px',
        lg: '12px',
        xl: '16px',
        card: '16px',
        inner: '12px',
        control: '8px',
        pill: '9999px',
      },
      boxShadow: {
        sm: '0 1px 2px rgb(0 0 0 / 0.05)',
        DEFAULT: '0 1px 3px rgb(0 0 0 / 0.06), 0 4px 12px rgb(0 0 0 / 0.06)',
        md: '0 2px 6px rgb(0 0 0 / 0.08), 0 8px 24px rgb(0 0 0 / 0.08)',
        lg: '0 4px 12px rgb(0 0 0 / 0.12), 0 16px 48px rgb(0 0 0 / 0.12)',
        card: 'var(--shadow-card)',
        glow: 'var(--shadow-glow)',
      },
      fontFamily: {
        sans: ['Inter', 'ui-sans-serif', 'system-ui', '-apple-system', 'Segoe UI', 'Roboto', 'sans-serif'],
        mono: ['IBM Plex Mono', 'ui-monospace', 'SFMono-Regular', 'Menlo', 'Consolas', 'monospace'],
      },
      letterSpacing: {
        tightest: '-0.05em',
        tighter2: '-0.031em',
        tighter: '-0.014em',
      },
      maxWidth: {
        page: '1600px',
      },
      spacing: {
        4.5: '1.125rem',
        18: '4.5rem',
      },
    },
  },
  plugins: [],
};