import { defineConfig, presetUno } from 'unocss'

export default defineConfig({
  presets: [presetUno()],
  theme: {
    colors: {
      party: {
        bg: '#0b0e17',
        panel: '#141a2a',
        accent: '#f43f5e',
        gold: '#fbbf24',
      },
    },
  },
  shortcuts: {
    'glass-panel': 'rounded-2xl border border-white/10 bg-white/5 backdrop-blur',
  },
})
