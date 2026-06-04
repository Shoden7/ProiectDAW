import type { Config } from 'tailwindcss'

const config: Config = {
  content: ['./src/**/*.{js,ts,jsx,tsx,mdx}'],
  theme: {
    extend: {
      fontFamily: {
        display: ['var(--font-display)'],
        body: ['var(--font-body)'],
        mono: ['var(--font-mono)'],
      },
      colors: {
        iron: {
          950: '#0a0a0b',
          900: '#111113',
          800: '#1a1a1f',
          700: '#242429',
          600: '#2e2e35',
          500: '#3d3d47',
          400: '#5a5a6a',
          300: '#8a8a9a',
          200: '#b4b4c4',
          100: '#e0e0ee',
        },
        blood: {
          DEFAULT: '#c0272d',
          dark: '#8a1a1e',
          light: '#e03038',
          glow: '#ff3040',
        },
        gold: {
          DEFAULT: '#c8973a',
          light: '#e8b45a',
          dim: '#8a6520',
        },
      },
      animation: {
        'flicker': 'flicker 3s infinite',
        'scan': 'scan 8s linear infinite',
        'pulse-red': 'pulse-red 2s ease-in-out infinite',
        'slide-up': 'slide-up 0.4s cubic-bezier(0.16, 1, 0.3, 1)',
        'fade-in': 'fade-in 0.3s ease-out',
      },
      keyframes: {
        flicker: {
          '0%, 100%': { opacity: '1' },
          '92%': { opacity: '1' },
          '93%': { opacity: '0.8' },
          '94%': { opacity: '1' },
          '96%': { opacity: '0.9' },
          '97%': { opacity: '1' },
        },
        scan: {
          '0%': { transform: 'translateY(-100%)' },
          '100%': { transform: 'translateY(100vh)' },
        },
        'pulse-red': {
          '0%, 100%': { boxShadow: '0 0 8px rgba(192,39,45,0.4)' },
          '50%': { boxShadow: '0 0 20px rgba(192,39,45,0.8)' },
        },
        'slide-up': {
          from: { opacity: '0', transform: 'translateY(12px)' },
          to: { opacity: '1', transform: 'translateY(0)' },
        },
        'fade-in': {
          from: { opacity: '0' },
          to: { opacity: '1' },
        },
      },
      backgroundImage: {
        'noise': "url(\"data:image/svg+xml,%3Csvg viewBox='0 0 200 200' xmlns='http://www.w3.org/2000/svg'%3E%3Cfilter id='noise'%3E%3CfeTurbulence type='fractalNoise' baseFrequency='0.9' numOctaves='4' stitchTiles='stitch'/%3E%3C/filter%3E%3Crect width='100%25' height='100%25' filter='url(%23noise)' opacity='0.4'/%3E%3C/svg%3E\")",
        'grid-iron': "linear-gradient(rgba(255,255,255,0.02) 1px, transparent 1px), linear-gradient(90deg, rgba(255,255,255,0.02) 1px, transparent 1px)",
      },
    },
  },
  plugins: [],
}
export default config
