/**
 * The whole app is dark themed - switch Quasar to dark mode on startup.
 * The body class is a fallback in case $q is not injected yet.
 */
export default defineNuxtPlugin((nuxtApp) => {
  const quasar = (nuxtApp.$q ?? {}) as { dark?: { set?: (value: boolean) => void } }
  quasar.dark?.set?.(true)
  document.body.classList.add('body--dark')
})
