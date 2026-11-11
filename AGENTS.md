# AGENTS

Purpose: concise conventions, architecture notes, and workflows for contributors and AI agents working in this monorepo (backend + frontend).

Contents
- Repository overview
- Quick rules (general)
- Backend (architecture, data, auth)
- Frontend (architecture, state, API consumption)
- Workflows (development, migrations, feature flow)
- Verification & final checklist

Repository overview

Layout:

```
/
├── backend/   # .NET 10, FastEndpoints, EF Core
└── client/    # Nuxt 4, Vue 3, TypeScript, Quasar
```

Backend communicates with frontend over /api/*; frontend is an SPA (ssr: false).

Quick rules (apply to all changes)

- Inspect existing code and nearby features first.
- Make the smallest coherent change that solves the task.
- Reuse existing composables, services, and components instead of creating new abstractions.
- Prefer simple, localised changes; avoid broad refactors during focused feature work.
- Never commit secrets, generated build output, or personal configs.
- Prefer classes over records for domain models and DTOs; use records only when immutability and value semantics are intentionally required.
- Use Mapster for object-to-object mapping wherever practical; prefer a centralized Mapster configuration augmented with feature-based registrations (maps colocated with feature folders) when that improves clarity, and prefer EF Core projection support instead of manual copying.

Backend

What belongs in backend:
- FastEndpoints endpoints, request/response models, validation
- Authentication, authorization, server-side business rules
- EF Core data access, migrations, seed data
- Application services, background jobs, file storage (IStorageService)

Structure (examples):

```
backend/
├── Program.cs
├── Database/AppDbContext.cs
├── Entities/
├── Features/
├── Services/
└── Migrations/
```

Backend rules (essentials):
- Keep endpoints and validators close to their feature.
- Use AppDbContext directly; do not introduce a generic repository unless required.
- Prefer async EF Core APIs and pass CancellationToken where available.
- All schema changes must have EF Core migrations; review migrations before committing.
- Enforce authorization server-side; do not rely on frontend checks.
- Use cookie authentication (current system); do not switch to JWT without requirement.
- Store files via IStorageService; validate filenames and prevent path traversal.
- Prefer classes over records for entities, request/response models, and services. Records may be used for small immutable value types but should not be the default for EF Core entities or mutable DTOs.
- Standardize mapping with Mapster: add and reuse a global Mapster configuration (TypeAdapterConfig) while allowing feature-based registration (map configurations colocated in feature folders); use Adapt/ProjectTo for DTO projections, and avoid handwritten property copying across layers when Mapster covers the mapping.

Frontend

What belongs in frontend:
- Pages, components, composables, UI and client-side orchestration.

Stack and conventions:
- Nuxt 4, Vue 3, TypeScript, Quasar, UnoCSS, Zod
- Project uses custom composable patterns instead of a global state library.

Primary composables:
- useForm — form state, validation, submission handling
- useRequest — standard API requests and lifecycle
- useRequestTable — tables/lists with pagination, filtering, sorting

Frontend rules (essentials):
- Prefer `<script setup lang="ts">` and Nuxt auto-imports.
- Reuse existing components and composables; search client/components and feature folders before adding new ones.
- Do not introduce Pinia, Vuex, or other global state libraries.
- Keep API calls simple: useRequest for typical calls, useRequestTable for table flows.
- Use Zod where runtime validation is needed; avoid adding another validation library.

Workflows

Development
- Backend: `pnpm run watch` or `dotnet watch --project backend`
- Frontend: `pnpm dev` (dev server usually at http://localhost:5101)
- Run backend and frontend in separate terminals.

Database / migrations
1. Modify EF Core model.
2. Create a new migration.
3. Review the migration.
4. Build and verify the backend.
5. Apply or deploy migrations following environment practices.

Feature implementation (typical full-stack flow)
1. Identify feature area and inspect similar features (backend + frontend).
2. Implement backend endpoint, validation, and create migration if required.
3. Implement frontend page/component, using existing composables and UI components.
4. Verify both backend and frontend; run build/linters/tests as appropriate.
5. Review final diff for unrelated changes.

Verification & final checklist

Before marking work done:
- Did you inspect and reuse existing patterns where possible?
- Backend: validation, authorization, EF Core usage, migration created when schema changed.
- Frontend: useForm/useRequest/useRequestTable where appropriate; reused Quasar components; no Pinia.
- Dependencies: no unnecessary libraries added; document and justify any new dependency.
- Run the relevant verification commands (build, lint, typecheck, tests) where available.
- Mapping: if you add or change mappings, include or update Mapster configuration (global or feature-level) and add a small test for non-trivial mappings. Prefer Mapster usage over manual mapping in new code unless there is a clear reason not to.

If anything in this doc conflicts with a concrete requirement or infra constraint, ask for clarification before making the change.

Useful files / locations

- backend/Program.cs
- backend/Database/AppDbContext.cs
- backend/Features/
- backend/Services/
- backend/Migrations/
- client/nuxt.config.ts
- client/components/
- client/composables/
