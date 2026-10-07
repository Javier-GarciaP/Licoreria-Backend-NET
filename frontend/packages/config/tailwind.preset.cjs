/**
 * Preset compartido de Tailwind para el frontend de Licorería / Discoteca.
 * Los colores apuntan a variables CSS (definidas en packages/ui/src/styles/theme.css)
 * para permitir el cambio de tema Azul UNET / Oscuro en caliente.
 */
module.exports = {
  theme: {
    extend: {
      colors: {
        canvas: 'rgb(var(--color-canvas) / <alpha-value>)',
        surface: 'rgb(var(--color-surface) / <alpha-value>)',
        elevated: 'rgb(var(--color-elevated) / <alpha-value>)',
        hairline: 'rgb(var(--color-hairline) / <alpha-value>)',
        ink: 'rgb(var(--color-ink) / <alpha-value>)',
        muted: 'rgb(var(--color-muted) / <alpha-value>)',
        stone: 'rgb(var(--color-stone) / <alpha-value>)',
        accent: 'rgb(var(--color-accent) / <alpha-value>)',
        'accent-soft': 'rgb(var(--color-accent-soft) / <alpha-value>)',
        'accent-ink': 'rgb(var(--color-accent-ink) / <alpha-value>)',
        violet: 'rgb(var(--color-violet) / <alpha-value>)',
        success: 'rgb(var(--color-success) / <alpha-value>)',
        'success-ink': 'rgb(var(--color-success-ink) / <alpha-value>)',
        warning: 'rgb(var(--color-warning) / <alpha-value>)',
        'warning-ink': 'rgb(var(--color-warning-ink) / <alpha-value>)',
        danger: 'rgb(var(--color-danger) / <alpha-value>)',
        'danger-ink': 'rgb(var(--color-danger-ink) / <alpha-value>)',
        info: 'rgb(var(--color-info) / <alpha-value>)',
        'info-ink': 'rgb(var(--color-info-ink) / <alpha-value>)',
        butter: 'rgb(var(--color-butter) / <alpha-value>)',
        'butter-ink': 'rgb(var(--color-butter-ink) / <alpha-value>)',
        'on-pastel': 'rgb(var(--color-on-pastel) / <alpha-value>)',
      },
      borderRadius: {
        card: '16px',
        inner: '12px',
        control: '8px',
        pill: '9999px',
      },
      boxShadow: {
        card: 'var(--shadow-card)',
        soft: 'var(--shadow-soft)',
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
