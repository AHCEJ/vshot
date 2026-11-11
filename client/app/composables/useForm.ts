import type { z } from 'zod'

/**
 * Form composable: local reactive values + zod validation + submit handling.
 */
export function useForm<TSchema extends z.ZodTypeAny>(options: {
  schema: TSchema
  initialValues: Partial<z.infer<TSchema>>
  onSubmit: (values: z.infer<TSchema>) => Promise<void> | void
}) {
  type Values = z.infer<TSchema>

  const values = reactive({ ...options.initialValues }) as Values
  const errors = reactive<Record<string, string>>({})
  const submitting = ref(false)

  async function submit() {
    for (const key of Object.keys(errors))
      delete errors[key]

    const result = options.schema.safeParse(values)
    if (!result.success) {
      for (const issue of result.error.issues) {
        const key = String(issue.path[0] ?? '_')
        if (!errors[key])
          errors[key] = issue.message
      }
      return
    }

    submitting.value = true
    try {
      await options.onSubmit(result.data as Values)
    }
    finally {
      submitting.value = false
    }
  }

  function reset() {
    Object.assign(values as Record<string, unknown>, options.initialValues)
    for (const key of Object.keys(errors))
      delete errors[key]
  }

  return { values, errors, submitting, submit, reset }
}
