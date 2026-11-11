/**
 * Standard request composable: wraps an async API call with loading and
 * error state. The caller decides what to do with the returned value.
 */
export function useRequest<T = unknown>() {
  const loading = ref(false)
  const error = ref<string | null>(null)

  async function request(fn: () => Promise<T>): Promise<T | undefined> {
    loading.value = true
    error.value = null
    try {
      return await fn()
    }
    catch (err: unknown) {
      error.value = apiErrorMessage(err)
    }
    finally {
      loading.value = false
    }
  }

  return { loading, error, request }
}
