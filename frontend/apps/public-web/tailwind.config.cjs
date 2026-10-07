/** @type {import('tailwindcss').Config} */
module.exports = {
  content: ['./index.html', './src/**/*.{ts,tsx}', '../../packages/ui/src/**/*.{ts,tsx}'],
  presets: [require('../../packages/config/tailwind.preset.cjs')],
  theme: {
    extend: {
      colors: {
        'tiger-gold': '#faae33',
        'ember-rust': '#823513',
        'saffron-glow': '#9f531b',
        'dark-spice': '#402011',
        'charred-clove': '#281006',
        'cardamom-brown': '#6b2e12',
        'chili-red': '#d1255c',
      },
      fontFamily: {
        display: ['Antonio', 'Druk Wide', 'Bebas Neue', 'Impact', 'ui-sans-serif', 'sans-serif'],
      },
      fontSize: {
        caption: ['11px', { lineHeight: '1.2', letterSpacing: '0.02em' }],
        subheading: ['18px', { lineHeight: '1.2', letterSpacing: '0.01em' }],
        'heading-sm': ['29px', { lineHeight: '1.1', letterSpacing: '-0.005em' }],
        heading: ['65px', { lineHeight: '0.95', letterSpacing: '-0.01em' }],
        'heading-lg': ['101px', { lineHeight: '0.9', letterSpacing: '-0.016em' }],
        display: ['195px', { lineHeight: '0.8', letterSpacing: '-0.02em' }],
      },
      maxWidth: {
        poster: '1440px',
      },
      borderRadius: {
        card: '6px',
        button: '9999px',
      },
    },
  },
};
