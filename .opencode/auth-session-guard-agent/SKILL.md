---
name: auth-session-guard-agent
description: Change authentication, authorization, permissions, or session behavior across the employee portal without weakening security controls.
---

# Auth and Session Guard Agent

Use this skill for authentication, authorization, permissions, or session changes. Apply `repository-implementation-conventions` for shared implementation and feature-slice rules.

## Security-relevant scope

- Backend user features: `backend/Features/User`.
- Admin/session features: `backend/Features/Admin/Sessions`, `backend/Features/Admin/Users`.
- Policies and role checks: `backend/Policies`, `backend/Enums`.
- Session/user services: `backend/Services/SessionValidationMiddleware.cs`, `backend/Services/UserService.cs`.
- Frontend auth gate: `client/middleware/1.auth.global.ts`.
- Frontend session state: `client/composables/user.ts` (`useState('user')`, `fetchUser`/`login`/`logout`).
- Frontend permission checks: `client/utils/check-permission.ts`, `client/auth/permission.ts`.
- User-facing security pages: `client/pages/user/*`, `client/pages/admin/sessions.vue`.

## Security responsibilities

- Keep backend authorization as the source of truth. Frontend guards complement but never replace server enforcement.
- Do not weaken policy checks or expose protected routes/data without an explicit requirement.
- Protect roles, permissions, session state, password/security fields, and audit data from unintended mapping or overposting. Keep authorization decisions out of mapping configuration.
- Maintain consistent unauthorized/forbidden responses and avoid leaking sensitive details.
- Prefer minimal, auditable changes. Trace effects across login, logout, profile/password changes, session validation, and admin session management as applicable.

## Delivery notes

- List flows/files changed and describe auth/session behavior before and after.
- Identify policy, role, and permission implications.
- Include focused security regression checks for affected flows.
