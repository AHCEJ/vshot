---
name: fullstack-portal-architect
description: Plan and implement cross-module employee portal features spanning .NET API slices and Nuxt pages/components while keeping contracts aligned.
---

# Fullstack Portal Architect

Use this skill for work that crosses backend and frontend feature boundaries. Apply `repository-implementation-conventions` for shared implementation rules, `backend-feature-agent` for endpoint-slice details, and `frontend-module-agent` for Nuxt feature details where relevant.

## Project map

- Backend feature slices: `backend/Features/{Admin,AssetManagement,Common,HRIS,My,User}`.
- Shared backend persistence: `backend/Entities`, `backend/Database`, `backend/Migrations`.
- Frontend routes: `client/pages/{admin,asset-management,common,hris,my,user}`.
- Frontend feature UI: `client/components/{admin,app,asset-management,common,hris,my,user}`.
- Shared frontend code: `client/composables`, `client/types`, `client/enums`, `client/options`, `client/config`, `client/auth`, `client/utils`, `client/middleware`, and `client/layouts`.

## Cross-stack responsibilities

1. Trace the complete feature flow from route and UI through relative `/api` calls to FastEndpoints and persistence.
2. Keep API routes, request/response models, validation, permissions, and frontend usage aligned.
3. Preserve existing URL, cookie-auth, and database behavior unless a planned requirement changes them.
4. Coordinate endpoint, frontend, and persistence changes as one coherent vertical slice; identify migrations or seed updates when needed.
5. For security-sensitive work, apply `auth-session-guard-agent` and do not treat frontend guards as backend authorization.

## Delivery notes

- State the backend and frontend feature slices touched.
- List route and API contract changes, if any.
- Identify authorization implications and migration/seed requirements.
- Include focused verification steps for the end-to-end flow.
