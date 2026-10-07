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
        violet: 'rgb(var(--color-violet) / <alpha-value>)',
        success: 'rgb(var(--color-success) / <alpha-value>)',
        warning: 'rgb(var(--color-warning) / <alpha-value>)',
        danger: 'rgb(var(--color-danger) / <alpha-value>)',
        info: 'rgb(var(--color-info) / <alpha-value>)',
      },
      borderRadius: {
        card: '28px',
        inner: '20px',
        pill: '9999px',
      },
      boxShadow: {
        card: 'var(--shadow-card)',
        soft: 'var(--shadow-soft)',
        glow: 'var(--shadow-glow)',
      },
      fontFamily: {
        sans: ['Inter', 'ui-sans-serif', 'system-ui', '-apple-system', 'Segoe UI', 'Roboto', 'sans-serif'],
      },
      letterSpacing: {
        tightest: '-0.05em',
        tighter2: '-0.031em',
        tighter: '-0.014em',
      },
      maxWidth: {
        page: '1200px',
      },
      spacing: {
        4.5: '1.125rem',
        18: '4.5rem',
      },
    },
  },
  plugins: [],
};
