---
name: frontend-module-agent
description: Build or refine Nuxt pages and Quasar feature components using Nuxt file-based routing and the repository's request and form composable patterns.
---

# Frontend Module Agent

Implement frontend behavior within the existing Nuxt application structure. Apply `repository-implementation-conventions` for shared Nuxt, composable, validation, API, and UI rules.

## Feature boundaries

- Nuxt route files live in `client/pages/{admin,asset-management,common,hris,my,user}` and determine URLs. Keep route files focused on page composition and route-specific metadata.
- Feature UI lives in the matching `client/components/{domain}` area. Reuse common components and shared composables when behavior is genuinely shared.
- Keep feature-specific state and API interactions with the feature when practical; keep truly shared types, composables, layouts, middleware, and utilities in their established shared locations.

## Task-specific responsibilities

- Endpoint calls stay untyped: pass plain closures to `useRequest`/`useRequestTable` without explicit generics, `api.get<T>()` type parameters, or `.data as T` casts.
- Do not add `: any` annotations to callbacks, parameters, or catch variables — implicit-any and unknown-catch checks are disabled (`client/nuxt.config.ts > typescript.tsConfig`), so untyped data needs no annotation.
- Declare types only when necessary, for contracts between components and pages (`defineProps`, `defineEmits`, `defineModel`, values passed into components). Keep them inline in the declaring file; never extract a type used only once or add a typed API layer.
- `useForm` fields reflect the endpoint request contract: type them through initial-value inference, a Zod schema mirroring the backend `Validator`, or an inline interface when the form crosses component boundaries.
- Preserve existing route and navigation behavior unless the request asks for a change.
- Maintain established list/detail/form interactions and usable mobile and desktop layouts.
- Avoid adding an alternate request, form, validation, or state-management abstraction.

## Delivery notes

- Identify page, component, composable, and type paths changed.
- Describe route/navigation behavior and API assumptions affected.
- Suggest manual list/detail/create/update checks appropriate to the change.
