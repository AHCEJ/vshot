---
name: repository-implementation-conventions
description: Use for general implementation and refactoring in this repository. Defines shared Vertical Slice Architecture, Nuxt conventions, styling, validation, reuse, and quality rules.
---

# Repository Implementation Conventions

Use this skill as the shared source of truth for implementation conventions. Task-specific skills should add their domain rules without repeating these shared rules.

## Architecture

- Backend features use Vertical Slice Architecture under `backend/Features/{Domain}/{Feature}/{Action}`. Keep endpoint handlers, request/response models, and validators close to the operation they serve. Keep persistence entities/configuration and migrations in their existing shared data areas; keep cross-cutting services only for behavior genuinely shared across slices.
- Slice file layout: each action directory contains `Endpoint.cs` (exactly one endpoint class, named `Endpoint`), `Validator.cs` (class `Validator`, holding the FluentValidation rules for the action's request), and `Models.cs` (only when the action has request/response DTOs). Inline `ThrowError` domain checks may stay in the endpoint, but request field rules belong in `Validator.cs`. Never declare request/response types inside `Endpoint.cs`. Grouping directories that own no endpoint of their own contain only child action directories plus their parent `Models.cs` when the feature shares models.
- Model placement: DTOs shared by several actions of a feature live in that feature's parent `Models.cs` (namespace `Backend.Features.{Domain}.{Feature}`); action-local DTOs live in `{Action}/Models.cs` (namespace `...{Feature}.{Action}`) and may inherit the parent model (for example `AssetRowRes : AssetModel`). Same-named types with different shapes belong to their own action namespaces; do not merge them.
- Frontend routes are Nuxt file-based routes in `client/pages`. Preserve this routing convention and existing URL/API contracts. Keep page files focused on route metadata and composition; place feature UI and behavior with the owning domain/module using current `client/components` and `client/composables` conventions. Do not introduce parallel state, request, or error-handling frameworks.
- Reuse existing abstractions when they fit; do not extract a generic abstraction solely to reduce line count. Keep domain-specific behavior inside its feature slice.
- Keep backend and frontend contracts aligned. Preserve API, database, and navigation names unless a change is required and planned.

## Backend conventions

- Follow existing FastEndpoints route, authorization, validation, EF Core, and response patterns.
- Use Mapster `Adapt<T>()` for suitable object mappings and `ProjectToType<T>()` for EF-translatable projections. Prefer convention-based nested-property flattening (for example, `Unit.Name` to `UnitName`) before adding explicit mapping configuration.
- Keep domain decisions, calculations, and lifecycle transitions explicit rather than hiding them in mapping. For updates, map only request-authorized fields; protect keys, audit fields, navigation properties, and lifecycle state. Keep authorization decisions outside mapping configuration.
- Validate domain constraints on the server, including referenced entities and uniqueness. Return clear field-level errors rather than raw database exceptions.
- Review nullability, relationships, indexes, and data/backfill implications for schema changes. Preserve audit, soft-delete, and lifecycle behavior.

## Frontend conventions

- Follow Nuxt file-based routing, auto-import, composable, and component conventions already configured in `client/`. Preserve the established Nuxt app structure unless an explicit migration is requested.
- Reuse `useForm`, `useRequest`, `useRequestTable`, `client/utils/api.ts`, and common components where suitable. API calls use relative `/api` paths and backend cookie authentication (`withCredentials: true`).
- Keep feature state with its owning feature unless it is genuinely shared. Keep request field names and response shapes consistent with backend contracts. Endpoint reads stay untyped: `useRequest`/`useRequestTable` closures without generics, no `api.get<T>()`, no `.data as T` casts. Declare types only for contracts between components and pages, inline at first use; never introduce a typed API layer. `useForm` fields mirror the endpoint request contract through initial-value inference, a Zod schema mirroring the backend `Validator`, or an inline interface when the form crosses component boundaries.
- `noImplicitAny` and `useUnknownInCatchVariables` are disabled (`typescript.tsConfig` in `client/nuxt.config.ts`), so untyped endpoint data flows as `any` on its own. Never add `: any` annotations to callbacks, function parameters, or catch variables. The one empty-array exception: a `const x = []` that receives `.push()` calls and later crosses into a template infers `never[]` and needs an inline declaration type at first use.
- Canonical data flow: load with `useRequest(() => api.get('/path').then(x => x.data))` or `useRequestTable` driven by QTable `onRequest`, triggered via `onMounted(() => x.submit())`; mutate with direct `api.post/put/delete`; surface errors through `getRequestErrorMessage` and Quasar notify (403 suppressed) or `useForm` field errors.
- Reuse Quasar patterns and existing visual conventions. Use `required` CSS classes for required-field markers; do not use a native `required` prop as a visual marker or append a manual `*` where the class already renders one. Keep labels and validation messages aligned with server rules.
- Display server validation errors on corresponding controls using the existing form composable API. For nested/indexed keys, use the form composable lookup rather than reading internal error storage.
- Before a visual change, compare sibling forms/views and fix equivalent in-scope instances for consistency.

## Styling conventions

- UI styling is Quasar components plus UnoCSS utilities configured in `client/uno.config.ts` (`presetWind4`, loaded through the `@unocss/nuxt` module). Quasar keeps owning what it defines: components and their `q-*` classes, and semantic/brand colors (`text-positive`, `bg-negative`, `text-primary`, `rounded-borders`).
- Uno-first going forward: in new or touched markup prefer Uno utilities (`text-sm`, `mb-4`, `flex`, `grid`, `gap-*`, `text-gray-*`, `dark:*`) over Quasar's utility classes (`text-caption`, `q-mb-md`, `text-grey`). Do not mass-convert existing markup; migrate Quasar utility classes only in code you are already changing.
- The UnoCSS spacing scale is numeric (`4` = 1rem) plus `xs`, `sm`, `lg`, `xl`, `2xl` … There is no `md` key, so `mb-md`/`gap-md` compile to nothing — this and other phantom classes (`text-black-1`, `border-grey-3`, `text-subtitle`, `q-py-xxs`) were dead in templates and have been removed. Prefer `mb-sm` for toolbar spacing to match existing tables, and `mb-4` where a full 1rem gap is meant.
- Never build Uno utility names at runtime. The `` :class="`text-${color}`" `` pattern is allowed only for Quasar semantic colors sourced from `client/options/*.ts`, because those classes ship inside Quasar's CSS; any other interpolated class name would not be extracted by UnoCSS.
- `dark:`/`light:` variants map to Quasar's `body--dark`/`body--light` body classes through the `dark` selectors in `client/uno.config.ts`. Do not add a `.dark` class or `prefers-color-scheme` dark mode.
- UnoCSS's preflight reset from `presetWind4` is intentionally enabled and is appended after Quasar's CSS; Quasar's class rules still win on specificity. Keep it, and do not add `@unocss/reset`.
- Verify styling changes with `pnpm --filter client lint`, which runs `@unocss/order` over class attributes.

## Workflow

1. Inspect the owning backend slice, frontend route/components, adjacent validators, and existing shared helpers before changing code.
2. Make cohesive changes within the feature slice and across its necessary API, UI, validation, and persistence boundaries.
3. Preserve behavior and user data; include migration guidance when persisted schema changes.
4. Review the final diff for accidental contract changes, duplicate logic/markers, and unrelated edits.
5. Run focused checks for touched areas and broader required checks when practical; distinguish pre-existing failures from regressions.
