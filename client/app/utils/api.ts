/**
 * Extracts a human-readable message from a $fetch/API error.
 * FastEndpoints wraps failures in { errors: [{ reason }] } - tolerate casing variants.
 */
export function apiErrorMessage(err: unknown, fallback = 'Something went wrong'): string {
  const data = (err as { data?: unknown })?.data as
    | {
        errors?: Array<{ reason?: string } | string>
        Errors?: Array<{ Reason?: string } | string>
        message?: string
        Message?: string
      }
    | undefined

  const first = (data?.errors?.[0] ?? data?.Errors?.[0]) as
    | { reason?: string, Reason?: string }
    | string
    | undefined
  if (typeof first === 'string')
    return first
  if (first?.reason)
    return first.reason
  if (first?.Reason)
    return first.Reason

  return data?.message ?? data?.Message ?? (err instanceof Error ? err.message : fallback)
}
