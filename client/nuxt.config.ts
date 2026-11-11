export default defineNuxtConfig({
  compatibilityDate: '2026-11-01',
  ssr: false,
  modules: ['nuxt-quasar-ui', '@unocss/nuxt'],
  devServer: { port: 5101 },
  // Flat component names: VideoTile instead of PartyVideoTile.
  components: [{ path: '~/components', pathPrefix: false }],
  quasar: {
    config: {
      brand: {
        primary: '#f43f5e',
        accent: '#fbbf24',
        dark: '#0b0e17',
        positive: '#22c55e',
        negative: '#ef4444',
        info: '#38bdf8',
        warning: '#f59e0b',
      },
    },
    plugins: ['Notify'],
    extras: {
      font: 'roboto-font',
      fontIcons: ['material-icons'],
    },
  },
  css: ['~/assets/css/main.css'],
  nitro: {
    devProxy: {
      // Nitro strips the route prefix before forwarding, so targets must
      // repeat it: /api/x -> http://localhost:5000/api/x
      '/api': { target: 'http://localhost:5000/api', changeOrigin: true },
      '/hubs': { target: 'http://localhost:5000/hubs', changeOrigin: true, ws: true },
    },
  },
  typescript: { strict: true },
})
